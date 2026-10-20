import { findDuplicate, findSimilar, normalizeCategory } from './category-name';

const categories = [
  { name: 'Network & VPN' },
  { name: 'Email & calendar' },
  { name: 'Hardware' },
];

describe('category names', () => {
  it('normalizes like the server', () => {
    expect(normalizeCategory('  Network   &   VPN ')).toBe('network and vpn');
    expect(normalizeCategory('Email/Calendar!')).toBe('email calendar');
  });

  it('treats "&" and "and" as the same name', () => {
    expect(findDuplicate('network and vpn', categories)?.name).toBe('Network & VPN');
  });

  it('suggests categories that share a word', () => {
    expect(findSimilar('vpn', categories).map((c) => c.name)).toEqual(['Network & VPN']);
    expect(findSimilar('calendar sync', categories).map((c) => c.name)).toEqual(['Email & calendar']);
  });

  it('gives no suggestions for an exact duplicate or an unrelated name', () => {
    expect(findSimilar('hardware', categories)).toEqual([]);
    expect(findSimilar('printing', categories)).toEqual([]);
  });
});
