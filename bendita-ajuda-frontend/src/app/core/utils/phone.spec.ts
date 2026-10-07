import { formatPhone, isValidPhone, phoneDigits } from './phone';

describe('phone', () => {
  it('formata celular e fixo enquanto digita', () => {
    expect(formatPhone('6')).toBe('(6');
    expect(formatPhone('6199')).toBe('(61) 99');
    expect(formatPhone('61999998888')).toBe('(61) 99999-8888');
    expect(formatPhone('6133334444')).toBe('(61) 3333-4444');
  });

  it('ignora o que não é número e limita a 11 dígitos', () => {
    expect(phoneDigits('(61) 99999-8888 123')).toBe('61999998888');
  });

  it('valida DDD + número', () => {
    expect(isValidPhone('(61) 99999-8888')).toBe(true);
    expect(isValidPhone('(61) 3333-4444')).toBe(true);
    expect(isValidPhone('(61) 9999')).toBe(false);
    expect(isValidPhone('01999998888')).toBe(false);
  });
});
