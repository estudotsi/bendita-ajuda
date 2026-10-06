import { MOCK_CATEGORIES } from './categories.mock';
import { MOCK_PROVIDERS } from './providers.mock';
import { searchProvidersMock } from './provider-search.mock';

const search = (text: string, categoryId?: string) =>
  searchProvidersMock(MOCK_PROVIDERS, MOCK_CATEGORIES, { text, categoryId });

const categoriesOf = (text: string) => new Set(search(text).map((p) => p.category.id));

describe('searchProvidersMock', () => {
  it('returns everyone when there is no filter', () => {
    expect(search('')).toHaveLength(MOCK_PROVIDERS.length);
  });

  it('finds plumbers by problem keywords', () => {
    expect(categoriesOf('vazamento')).toEqual(new Set(['encanador']));
    expect(categoriesOf('minha pia está vazando')).toEqual(new Set(['encanador']));
    expect(categoriesOf('DESCARGA')).toEqual(new Set(['encanador']));
  });

  it('finds electricians ignoring accents and case', () => {
    expect(categoriesOf('Chuveiro')).toEqual(new Set(['eletricista']));
    expect(categoriesOf('tomada')).toEqual(new Set(['eletricista']));
    expect(categoriesOf('elétrica')).toEqual(new Set(['eletricista']));
  });

  it('matches the category name while typing', () => {
    expect(categoriesOf('encan')).toEqual(new Set(['encanador']));
    expect(categoriesOf('tecnico')).toEqual(new Set(['tecnico-ar-condicionado']));
  });

  it('filters by category', () => {
    const result = search('', 'pintor');
    expect(result.length).toBeGreaterThan(0);
    expect(result.every((p) => p.category.id === 'pintor')).toBe(true);
  });

  it('returns nothing for unknown words', () => {
    expect(search('astronauta')).toHaveLength(0);
  });

  it('lists people from the chosen neighborhood first', () => {
    const result = searchProvidersMock(MOCK_PROVIDERS, MOCK_CATEGORIES, { neighborhood: 'Guará' });
    expect(result[0].neighborhood).toBe('Guará');
  });
});
