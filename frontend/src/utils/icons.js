import { createIcons, icons } from 'lucide';

export function renderIcons(root = document) {
  try {
    createIcons({
      icons,
      nameAttr: 'data-lucide',
      attrs: {
        'stroke-width': 2,
      },
    });
  } catch (e) {
    // Graceful fallback
  }
}
