export function normalizeBrazilianPhoneToE164(phone: string | null | undefined): string | null {
  if (!phone) {
    return null;
  }

  let digits = phone.replace(/\D/g, '');

  if (!digits) {
    return null;
  }

  if (digits.startsWith('00')) {
    digits = digits.slice(2);
  }

  if (digits.startsWith('55')) {
    digits = digits.slice(2);
  }

  if (digits.length < 10 || digits.length > 11) {
    return null;
  }

  return `+55${digits}`;
}

export function normalizeBrazilianPhoneForWhatsApp(phone: string | null | undefined): string {
  return normalizeBrazilianPhoneToE164(phone)?.replace(/^\+/, '') ?? '';
}

export function formatBrazilianPhoneInput(phone: string | null | undefined): string {
  const digits = (phone ?? '')
    .replace(/\D/g, '')
    .replace(/^00/, '')
    .replace(/^55/, '')
    .slice(0, 11);

  if (digits.length <= 2) {
    return digits ? `(${digits}` : '';
  }

  if (digits.length <= 6) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  }

  if (digits.length <= 10) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
  }

  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}
