// Сладкий мост — интерактивные сценарии.
// Модуль подключается с defer, инлайн-скриптов нет (требование строгой CSP).

// Меню на мобильных: открытие, закрытие по Esc и клику вне меню.
function initNavMenu() {
  const toggle = document.querySelector('[data-menu-toggle]');
  const menu = document.querySelector('[data-menu]');

  if (!toggle || !menu) {
    return;
  }

  const close = () => {
    menu.classList.remove('is-open');
    toggle.setAttribute('aria-expanded', 'false');
    toggle.setAttribute('aria-label', 'Открыть меню');
  };

  const open = () => {
    menu.classList.add('is-open');
    toggle.setAttribute('aria-expanded', 'true');
    toggle.setAttribute('aria-label', 'Закрыть меню');
  };

  toggle.addEventListener('click', () => {
    if (menu.classList.contains('is-open')) {
      close();
    } else {
      open();
    }
  });

  document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && menu.classList.contains('is-open')) {
      close();
      toggle.focus();
    }
  });

  document.addEventListener('click', event => {
    if (!menu.classList.contains('is-open')) {
      return;
    }

    if (!menu.contains(event.target) && !toggle.contains(event.target)) {
      close();
    }
  });
}

// Аккордеон: раскрытие по нажатию, состояние видно в aria-expanded.
function initAccordion() {
  document.querySelectorAll('[data-accordion-trigger]').forEach(trigger => {
    trigger.addEventListener('click', () => {
      const panel = document.getElementById(trigger.getAttribute('aria-controls'));

      if (!panel) {
        return;
      }

      const expanded = trigger.getAttribute('aria-expanded') === 'true';
      trigger.setAttribute('aria-expanded', expanded ? 'false' : 'true');
      panel.hidden = expanded;
    });
  });
}

// Степпер количества: кнопки плюс/минус вокруг числового поля.
function initSteppers() {
  document.querySelectorAll('[data-stepper]').forEach(stepper => {
    const input = stepper.querySelector('[data-stepper-input]');

    if (!input) {
      return;
    }

    const min = parseInt(stepper.getAttribute('data-min') || '1', 10);
    const max = parseInt(stepper.getAttribute('data-max') || '99', 10);

    function commit(value) {
      const next = Math.min(max, Math.max(min, value));
      input.value = String(next);
      input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    stepper.querySelectorAll('[data-stepper-step]').forEach(button => {
      button.addEventListener('click', () => {
        const delta = parseInt(button.getAttribute('data-stepper-step'), 10);
        commit(parseInt(input.value, 10) + delta);
      });
    });

    input.addEventListener('change', () => commit(parseInt(input.value, 10) || min));
  });
}

// Полосы заполнения: ширина ставится через CSSOM, без inline-атрибутов в разметке.
function initProgress() {
  document.querySelectorAll('[data-progress]').forEach(bar => {
    const max = parseFloat(bar.getAttribute('data-progress-max') || '100');
    const value = parseFloat(bar.getAttribute('data-progress-value') || '0');
    const ratio = max > 0 ? Math.max(0, Math.min(1, value / max)) : 0;
    const fill = bar.querySelector('[data-progress-fill]');

    if (fill) {
      fill.style.width = ratio > 0 ? `${Math.max(4, ratio * 100).toFixed(1)}%` : '0%';
    }
  });
}

// Трекер статусов заказа.
function initTracker() {
  document.querySelectorAll('[data-tracker]').forEach(tracker => {
    const current = parseInt(tracker.getAttribute('data-tracker-current') || '0', 10);
    const total = parseInt(tracker.getAttribute('data-tracker-total') || '5', 10);
    const steps = tracker.querySelectorAll('[data-tracker-step]');

    steps.forEach((step, index) => {
      if (index < current) {
        step.classList.add('is-done');
      }

      if (index === current) {
        step.classList.add('is-current');
      }
    });

    const fill = tracker.querySelector('[data-tracker-fill]');
    const percentLabel = tracker.querySelector('[data-tracker-percent]');
    const percent = total > 0 ? ((current + 1) / total) * 100 : 0;

    if (fill) {
      fill.style.width = `${percent.toFixed(0)}%`;
    }

    if (percentLabel) {
      percentLabel.textContent = `${percent.toFixed(0)}%`;
    }
  });
}

// Переключатель «Это подарок» и подобные чекбоксы.
function initSwitches() {
  document.querySelectorAll('[data-switch]').forEach(toggle => {
    const selector = toggle.getAttribute('data-switch-target');
    const target = selector ? document.querySelector(selector) : null;

    if (!target) {
      return;
    }

    function apply() {
      const isOn = toggle.type === 'checkbox'
        ? toggle.checked
        : toggle.getAttribute('aria-pressed') === 'true';

      target.hidden = !isOn;
      target.querySelectorAll('input, select, textarea').forEach(field => {
        field.disabled = !isOn;
      });
    }

    toggle.addEventListener('change', apply);

    if (toggle.type === 'button') {
      toggle.addEventListener('click', () => {
        const pressed = toggle.getAttribute('aria-pressed') === 'true';
        toggle.setAttribute('aria-pressed', pressed ? 'false' : 'true');
        apply();
      });
    }

    apply();
  });
}

// Поля с подсказками об ошибках: подсветка только после ухода фокуса.
function initFieldValidation() {
  document.querySelectorAll('[data-field-control]').forEach(control => {
    const wrap = control.closest('.field');

    if (!wrap) {
      return;
    }

    control.addEventListener('blur', () => {
      if (control.value.length === 0 && !control.hasAttribute('required')) {
        wrap.classList.remove('field--invalid');
        return;
      }

      wrap.classList.toggle('field--invalid', !control.checkValidity());
    });
  });
}

// После неудачной отправки фокус переходит на первое поле с ошибкой.
function initErrorFocus() {
  document.querySelectorAll('form').forEach(form => {
    const error = form.querySelector('.field__error:not(:empty), .field-validation-error:not(:empty)');

    if (!error) {
      return;
    }

    const described = [...form.querySelectorAll('[aria-describedby]')]
      .find(control => (control.getAttribute('aria-describedby') || '').split(' ').includes(error.id));

    const control = described || form.querySelector('[aria-invalid="true"]');
    control?.focus();
  });
}
// Показать/скрыть пароль: одна кнопка управляет всеми полями формы.
function initPasswordToggles() {
  document.querySelectorAll('[data-password-reveal]').forEach(button => {
    const form = button.closest('form') || document;
    const ids = (button.getAttribute('aria-controls') || '')
      .split(' ')
      .filter(id => id.length > 0);

    const fields = ids.length > 0
      ? ids.map(id => document.getElementById(id)).filter(field => field !== null)
      : [...form.querySelectorAll('input[type="password"]')];

    button.setAttribute('aria-pressed', 'false');

    button.addEventListener('click', () => {
      const willShow = button.getAttribute('aria-pressed') !== 'true';

      fields.forEach(field => {
        field.type = willShow ? 'text' : 'password';
      });

      button.setAttribute('aria-pressed', willShow ? 'true' : 'false');
      button.textContent = willShow ? 'Скрыть' : 'Показать';
    });
  });

  // При отправке формы пароли снова должны скрываться.
  document.querySelectorAll('form').forEach(form => {
    form.addEventListener('submit', () => {
      form.querySelectorAll('input[type="text"][data-password-field]').forEach(field => {
        field.type = 'password';
      });
    });
  });
}

function ready(callback) {
  if (document.readyState !== 'loading') {
    callback();
    return;
  }

  document.addEventListener('DOMContentLoaded', callback);
}

ready(() => {
  initNavMenu();
  initAccordion();
  initSteppers();
  initProgress();
  initTracker();
  initSwitches();
  initFieldValidation();
  initPasswordToggles();
  initErrorFocus();
});