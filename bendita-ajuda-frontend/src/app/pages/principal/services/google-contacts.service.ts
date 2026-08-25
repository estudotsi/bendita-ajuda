import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, NgZone } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { normalizeBrazilianPhoneToE164 } from '../../../shared/utils/phone-normalizer';

interface GoogleIdentityBrowser {
  google?: {
    accounts?: {
      oauth2?: {
        initTokenClient(config: GoogleTokenClientConfig): GoogleTokenClient;
      };
    };
  };
}

interface GoogleTokenClientConfig {
  client_id: string;
  scope: string;
  callback: (response: GoogleTokenResponse) => void;
  error_callback?: (error: unknown) => void;
}

interface GoogleTokenClient {
  requestAccessToken(options?: { prompt?: string }): void;
}

interface GoogleTokenResponse {
  access_token?: string;
  error?: string;
  error_description?: string;
}

interface GooglePeopleConnectionsResponse {
  connections?: GooglePerson[];
  nextPageToken?: string;
}

interface GooglePerson {
  names?: Array<{
    displayName?: string;
  }>;
  phoneNumbers?: Array<{
    canonicalForm?: string;
    value?: string;
  }>;
}

@Injectable()
export class GoogleContactsService {
  private readonly peopleApiUrl = 'https://people.googleapis.com/v1/people/me/connections';
  private readonly contactsScope = 'https://www.googleapis.com/auth/contacts.readonly';
  private scriptLoadPromise?: Promise<void>;

  constructor(
    private readonly http: HttpClient,
    private readonly zone: NgZone,
  ) {}

  async getNormalizedContactPhones(): Promise<Map<string, string>> {
    const accessToken = await this.requestContactsAccessToken();
    const phones = new Map<string, string>();
    let pageToken: string | undefined;

    do {
      const response = await this.getConnectionsPage(accessToken, pageToken);

      for (const person of response.connections ?? []) {
        const contactName = person.names?.find((name) => !!name.displayName)?.displayName || 'Contato';

        for (const phoneNumber of person.phoneNumbers ?? []) {
          const normalizedPhone = normalizeBrazilianPhoneToE164(
            phoneNumber.canonicalForm || phoneNumber.value,
          );

          if (normalizedPhone && !phones.has(normalizedPhone)) {
            phones.set(normalizedPhone, contactName);
          }
        }
      }

      pageToken = response.nextPageToken;
    } while (pageToken);

    return phones;
  }

  private async requestContactsAccessToken(): Promise<string> {
    await this.loadGoogleIdentityScript();

    return new Promise((resolve, reject) => {
      const oauth2 = this.getGoogleOAuth2();

      if (!oauth2) {
        reject(new Error('Google Identity Services nao foi carregado.'));
        return;
      }

      const tokenClient = oauth2.initTokenClient({
        client_id: environment.googleClientId,
        scope: this.contactsScope,
        callback: (response) => {
          this.zone.run(() => {
            if (response.error || !response.access_token) {
              reject(
                new Error(response.error_description || response.error || 'Permissao nao concedida.'),
              );
              return;
            }

            resolve(response.access_token);
          });
        },
        error_callback: (error) => {
          this.zone.run(() => reject(error));
        },
      });

      tokenClient.requestAccessToken({ prompt: 'consent' });
    });
  }

  private getConnectionsPage(
    accessToken: string,
    pageToken?: string,
  ): Promise<GooglePeopleConnectionsResponse> {
    let params = new HttpParams()
      .set('personFields', 'names,emailAddresses,phoneNumbers')
      .set('pageSize', '1000');

    if (pageToken) {
      params = params.set('pageToken', pageToken);
    }

    return firstValueFrom(
      this.http.get<GooglePeopleConnectionsResponse>(this.peopleApiUrl, {
        headers: new HttpHeaders({
          Authorization: `Bearer ${accessToken}`,
        }),
        params,
      }),
    );
  }

  private loadGoogleIdentityScript(): Promise<void> {
    if (this.scriptLoadPromise) {
      return this.scriptLoadPromise;
    }

    this.scriptLoadPromise = new Promise((resolve, reject) => {
      if (this.getGoogleOAuth2()) {
        resolve();
        return;
      }

      const existingScript = document.querySelector<HTMLScriptElement>(
        'script[src="https://accounts.google.com/gsi/client"]',
      );

      if (existingScript) {
        existingScript.addEventListener('load', () => resolve(), { once: true });
        existingScript.addEventListener('error', () => reject(new Error('Falha ao carregar Google.')), {
          once: true,
        });
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('Falha ao carregar Google.'));

      document.head.appendChild(script);
    });

    return this.scriptLoadPromise;
  }

  private getGoogleOAuth2():
    | NonNullable<NonNullable<GoogleIdentityBrowser['google']>['accounts']>['oauth2']
    | undefined {
    return (window as unknown as GoogleIdentityBrowser).google?.accounts?.oauth2;
  }
}
