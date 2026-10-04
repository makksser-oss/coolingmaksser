# LOVINGCOOL Studio Native

Нативное приложение для Windows 11 и LOVINGCOOL 6PRO 480×480 (USB VID_33C3 / PID_7791).

## Возможности
- C# / .NET 8 WinForms, без Python;
- self-contained single-file EXE;
- автоматический поиск COM и прямой протокол дисплея;
- коррекция ориентации 90° влево по умолчанию;
- полный кадр 480×480;
- LibreHardwareMonitor: CPU/GPU/RAM/SSD/плата/вентиляторы/частоты/мощность/напряжения;
- drag-and-drop виджеты;
- зеркало основного монитора;
- фон, текст, часы;
- системный трей и автозапуск.

## Сборка
GitHub Actions автоматически собирает Windows x64 EXE и публикует его как artifact **LOVINGCOOL-Studio-Windows-x64**.

Локально: запустите `build-exe.ps1` на Windows с .NET 8 SDK.

> Настоящий режим «Расширить рабочий стол» требует отдельного Windows Indirect Display Driver (IDD).