// Сладкий мост — интерактивные сценарии без фреймворков.
// Все значения приходят из data-атрибутов, inline-стилей и inline-скриптов нет (требование CSP).

(function () {
  'use strict';

  function ready(fn) {
    if (document.readyState !== 'loading') {
      fn();
      return;
    }
    document.addEventListener('DOMContentLoaded', fn);
  }

  // Мобильное меню.
  function initMenu() {
    var toggle = document.querySelector('[data-menu-toggle]');
    var nav = document.querySelector('[data-menu]');
    if (!toggle || !nav) {
      return;
    }

    toggle.addEventListener('click', function () {
      var isOpen = nav.classList.toggle('is-open');
      toggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    });
  }

  // Аккордеон: раскрытие по нажатию, состояние видно в aria-expanded.
  function initAccordion() {
    document.querySelectorAll('[data-accordion-trigger]').forEach(function (trigger) {
      trigger.addEventListener('click', function () {
        var panel = document.getElementById(trigger.getAttribute('aria-controls'));
        if (!panel) {
          return;
        }
        var expanded = trigger.getAttribute('aria-expanded') === 'true';
        trigger.setAttribute('aria-expanded', expanded ? 'false' : 'true');
        panel.hidden = expanded;
      });
    });
  }

  // Степпер количества: кнопки плюс/минус вокруг числового поля.
  function initSteppers() {
    document.querySelectorAll('[data-stepper]').forEach(function (stepper) {
      var input = stepper.querySelector('[data-stepper-input]');
      if (!input) {
        return;
      }

      var min = parseInt(stepper.getAttribute('data-min') || '1', 10);
      var max = parseInt(stepper.getAttribute('data-max') || '99', 10);

      function commit(value) {
        var next = Math.min(max, Math.max(min, value));
        input.value = String(next);
        input.dispatchEvent(new Event('change', { bubbles: true }));
      }

      stepper.querySelectorAll('[data-stepper-step]').forEach(function (button) {
        button.addEventListener('click', function () {
          var delta = parseInt(button.getAttribute('data-stepper-step'), 10);
          commit(parseInt(input.value, 10) + delta);
        });
      });

      input.addEventListener('change', function () {
        commit(parseInt(input.value, 10) || min);
      });
    });
  }

  // Прогресс-бар: значение берётся из data-value и выставляется через CSSOM.
  function initProgress() {
    document.querySelectorAll('[data-progress]').forEach(function (bar) {
      var max = parseFloat(bar.getAttribute('data-progress-max') || '100');
      var value = parseFloat(bar.getAttribute('data-progress-value') || '0');
      var ratio = max > 0 ? Math.max(0, Math.min(1, value / max)) : 0;
      var fill = bar.querySelector('[data-progress-fill]');
      if (fill) {
        fill.style.width = (ratio * 100).toFixed(1) + '%';
      }

      var label = document.querySelector('[data-progress-label]');
      if (label && label.getAttribute('data-progress-label') === bar.id) {
        label.textContent = Math.round(ratio * 100) + '%';
      }
    });
  }

  // Таймлайн заказа: подсветка шагов и процент выполнения.
  function initTracker() {
    document.querySelectorAll('[data-tracker]').forEach(function (tracker) {
      var current = parseInt(tracker.getAttribute('data-tracker-current') || '0', 10);
      var total = parseInt(tracker.getAttribute('data-tracker-total') || '5', 10);
      var steps = tracker.querySelectorAll('[data-tracker-step]');

      steps.forEach(function (step, index) {
        if (index < current) {
          step.classList.add('is-done');
        }
        if (index === current) {
          step.classList.add('is-current');
        }
      });

      var fill = tracker.querySelector('[data-tracker-fill]');
      if (fill) {
        var percent = total > 0 ? ((current + 1) / total) * 100 : 0;
        fill.style.width = percent.toFixed(0) + '%';
      }

      var percentLabel = tracker.querySelector('[data-tracker-percent]');
      if (percentLabel) {
        var value = total > 0 ? ((current + 1) / total) * 100 : 0;
        percentLabel.textContent = value.toFixed(0) + '%';
      }
    });
  }

  // Поля с подсказками об ошибках.
  function initFieldValidation() {
    document.querySelectorAll('[data-field-control]').forEach(function (control) {
      control.addEventListener('blur', function () {
        var wrapper = control.closest('.field');
        if (wrapper) {
          wrapper.classList.toggle('field--invalid', !control.checkValidity());
        }
      });
    });
  }

  // Переключатель «Это подарок» и прочие checkbox-переключатели.
  function initSwitches() {
    document.querySelectorAll('[data-switch]').forEach(function (toggle) {
      var targetSelector = toggle.getAttribute('data-switch-target');
      var target = targetSelector ? document.querySelector(targetSelector) : null;
      if (!target) {
        return;
      }

      function apply() {
        var isOn = toggle.type === 'checkbox' ? toggle.checked : toggle.getAttribute('aria-pressed') === 'true';
        target.hidden = !isOn;
        target.querySelectorAll('input, select, textarea').forEach(function (field) {
          field.disabled = !isOn;
        });
      }

      toggle.addEventListener('change', apply);
      if (toggle.type === 'button') {
        toggle.addEventListener('click', function () {
          var pressed = toggle.getAttribute('aria-pressed') === 'true';
          toggle.setAttribute('aria-pressed', pressed ? 'false' : 'true');
          apply();
        });
      }
      apply();
    });
  }

  // Показ пароля.
  function initPasswordToggles() {
    document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
      button.addEventListener('click', function () {
        var input = document.getElementById(button.getAttribute('data-password-toggle'));
        if (!input) {
          return;
        }
        var isPassword = input.type === 'password';
        input.type = isPassword ? 'text' : 'password';
        button.setAttribute('aria-label', isPassword ? 'Скрыть пароль' : 'Показать пароль');
      });
    });
  }

  // Отправка форм, зависящих от чекбоксов (например, выбор вклада).
  function initAutoSubmit() {
    document.querySelectorAll('[data-autosubmit]').forEach(function (form) {
      form.addEventListener('change', function () {
        form.submit();
      });
    });
  }

  ready(function () {
    initMenu();
    initAccordion();
    initSteppers();
    initProgress();
    initTracker();
    initFieldValidation();
    initSwitches();
    initPasswordToggles();
    initAutoSubmit();
  });
})();
