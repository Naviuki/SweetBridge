// Небольшие помощники интерфейса SweetWave: переключение блока получателя
// и подсказка про обязательную термоупаковку. Сервер всегда пересчитывает заказ сам.
(function () {
    'use strict';

    function bindGiftToggle() {
        var giftSwitch = document.querySelector('[data-gift-switch]');
        var recipientBlock = document.querySelector('[data-recipient-block]');

        if (!giftSwitch || !recipientBlock) {
            return;
        }

        function update() {
            var isGift = giftSwitch.checked;

            recipientBlock.classList.toggle('d-none', !isGift);

            recipientBlock.querySelectorAll('input, textarea').forEach(function (field) {
                field.disabled = !isGift;
            });
        }

        giftSwitch.addEventListener('change', update);
        update();
    }

    function bindWaveSelection() {
        var table = document.querySelector('[data-wave-table]');

        if (!table) {
            return;
        }

        var note = document.querySelector('[data-thermo-note]');
        var checkbox = document.querySelector('[data-thermo-checkbox]');

        function update() {
            var selected = table.querySelector('input[name="Input.WaveId"]:checked');

            if (!selected) {
                return;
            }

            var row = selected.closest('tr');
            var isRequired = row.dataset.thermoRequired === 'true';

            if (note) {
                note.textContent = isRequired
                    ? 'Для этой волны термоупаковка обязательна: в заказе есть шоколад, а месяц отправки требует защиты от холода или жары.'
                    : 'Термоупаковка по желанию: она защитит шоколад в жару и мороз.';
            }

            if (checkbox) {
                checkbox.checked = isRequired;
                checkbox.disabled = isRequired;
            }
        }

        table.querySelectorAll('input[name="Input.WaveId"]').forEach(function (radio) {
            radio.addEventListener('change', update);
        });

        update();
    }

    document.addEventListener('DOMContentLoaded', function () {
        bindGiftToggle();
        bindWaveSelection();
    });
})();