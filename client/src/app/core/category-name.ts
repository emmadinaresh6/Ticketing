// Same rule as Category.Normalize on the server: lower case, "&" as "and", punctuation and extra spaces removed.
export function normalizeCategory(name: string): string {
  return name
    .toLowerCase()
    .replace(/&/g, ' and ')
    .replace(/[^a-z0-9]+/g, ' ')
    .trim();
}

export function findDuplicate<T extends { name: string }>(draft: string, categories: T[]): T | undefined {
  const key = normalizeCategory(draft);
  return key ? categories.find((c) => normalizeCategory(c.name) === key) : undefined;
}

// Categories that share a word of 3+ letters with the draft, or contain it. Shown as "Did you mean...".
export function findSimilar<T extends { name: string }>(draft: string, categories: T[]): T[] {
  const key = normalizeCategory(draft);
  if (!key || findDuplicate(draft, categories)) return [];
  const words = key.split(' ').filter((w) => w.length >= 3 && w !== 'and');
  return categories.filter((c) => {
    const name = normalizeCategory(c.name);
    return name.includes(key) || words.some((w) => name.includes(w));
  });
}
