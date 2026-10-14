// Подгрузка товаров каталога по кнопке «Показать ещё».
// Запрос идёт на partial-обработчик страницы каталога, дубликаты отбрасываются по id.

(() => {
  'use strict';

  const button = document.querySelector('[data-load-more]');
  const grid = document.querySelector('[data-products-grid]');
  const counter = document.querySelector('[data-products-counter]');
  const status = document.querySelector('[data-products-status]');

  if (!button || !grid) {
    return;
  }

  let busy = false;

  function announce(message) {
    if (status) {
      status.textContent = message;
    }
  }

  function setBusy(isBusy) {
    busy = isBusy;
    button.disabled = isBusy;
    button.classList.toggle('is-loading', isBusy);
    button.textContent = isBusy ? 'Загружаем…' : 'Показать ещё';
  }

  function updateCounter() {
    if (!counter) {
      return;
    }

    const shown = grid.querySelectorAll('[data-product-id]').length;
    const total = parseInt(counter.getAttribute('data-total') || '0', 10);

    counter.textContent = `Показано ${shown} из ${total} товаров`;
  }

  function showError(retry) {
    const box = document.createElement('p');
    box.className = 'flash flash--error catalog-load-error';
    box.setAttribute('role', 'alert');
    box.append('Не удалось загрузить товары. ');

    const again = document.createElement('button');
    again.type = 'button';
    again.className = 'btn btn--ghost btn--sm';
    again.textContent = 'Повторить';
    again.addEventListener('click', () => {
      box.remove();
      load();
    });

    box.append(again);
    grid.parentElement.insertBefore(box, grid.nextSibling);
  }

  async function load() {
    if (busy) {
      return;
    }

    const nextUrl = button.getAttribute('data-next-url');

    if (!nextUrl) {
      button.hidden = true;
      return;
    }

    setBusy(true);
    announce('Загружаем товары…');

    try {
      const response = await fetch(nextUrl, {
        headers: { 'X-Requested-With': 'fetch' },
        credentials: 'same-origin'
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const html = await response.text();
      const fragment = document.createElement('div');
      fragment.innerHTML = html.trim();

      const incoming = fragment.querySelectorAll('[data-product-id]');
      const known = new Set([...grid.querySelectorAll('[data-product-id]')].map(node => node.dataset.productId));

      let added = 0;
      incoming.forEach(card => {
        if (known.has(card.dataset.productId)) {
          return;
        }

        known.add(card.dataset.productId);
        grid.append(card);
        added += 1;
      });

      const next = fragment.querySelector('[data-next-url]');
      if (next && added > 0) {
        button.setAttribute('data-next-url', next.getAttribute('data-next-url'));
      } else {
        button.hidden = true;
      }

      updateCounter();
      announce(added > 0 ? `Добавлено товаров: ${added}` : 'Новых товаров нет');

      if (added > 0) {
        const firstNew = grid.querySelector('[data-product-id]:last-of-type');
        if (firstNew) {
          firstNew.setAttribute('tabindex', '-1');
          firstNew.focus({ preventScroll: true });
        }
      }
    } catch (error) {
      announce('Ошибка загрузки товаров');
      showError();
    } finally {
      setBusy(false);
    }
  }

  // Ссылка остаётся рабочей без JS: без обработчика браузер просто перейдёт по href.
  button.addEventListener('click', event => {
    event.preventDefault();
    load();
  });

  updateCounter();
})();