import { HttpErrorResponse } from '@angular/common/http';

interface ApiErrorBody {
  code?: unknown;
  mensagem?: unknown;
  description?: unknown;
  errors?: unknown;
}

export function getHttpErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (typeof error.error === 'string' && error.error.trim()) {
    return error.error;
  }

  if (Array.isArray(error.error)) {
    const descriptions = error.error
      .map((item: ApiErrorBody) => getIdentityErrorMessage(item))
      .filter((description): description is string => !!description);

    if (descriptions.length) {
      return descriptions.join(' ');
    }
  }

  const body = error.error as ApiErrorBody | null;

  if (typeof body?.mensagem === 'string' && body.mensagem.trim()) {
    return body.mensagem;
  }

  if (body?.errors && typeof body.errors === 'object') {
    const validationMessages = Object.values(body.errors)
      .flatMap((value) => (Array.isArray(value) ? value : [value]))
      .filter(
        (value): value is string => typeof value === 'string' && value.trim().length > 0,
      );

    if (validationMessages.length) {
      return validationMessages.join(' ');
    }
  }

  return fallback;
}

function getIdentityErrorMessage(error: ApiErrorBody): string | null {
  const messages: Record<string, string> = {
    PasswordRequiresDigit: 'A senha deve conter pelo menos um número.',
    PasswordRequiresLower: 'A senha deve conter pelo menos uma letra minúscula.',
    PasswordRequiresNonAlphanumeric: 'A senha deve conter pelo menos um caractere especial.',
    PasswordRequiresUniqueChars: 'A senha deve conter mais caracteres diferentes.',
    PasswordRequiresUpper: 'A senha deve conter pelo menos uma letra maiúscula.',
    PasswordTooShort: 'A senha deve ter pelo menos 6 caracteres.',
  };
  const code = typeof error?.code === 'string' ? error.code : '';

  if (messages[code]) {
    return messages[code];
  }

  return typeof error?.description === 'string' && error.description.trim()
    ? error.description
    : null;
}
