import { HttpErrorResponse } from '@angular/common/http';

interface ApiErrorBody {
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
      .map((item: ApiErrorBody) => item?.description)
      .filter(
        (description): description is string =>
          typeof description === 'string' && description.trim().length > 0,
      );

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
