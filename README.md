en:
###This plugin NEVER overwrites the config when you reboot!
The config is generated ONCE when you first start the plugin.
After that, the plugin ONLY reads the config.

### Installation
1. Make sure that Oxide.Compiler.exe is in the root of your server
 If it is not, download it:
 Windows: https://downloads.oxidemod.com/artifacts/Oxide.Compiler/master/win-x64.Compiler.exe
 Linux: https://downloads.oxidemod.com/artifacts/Oxide.Compiler/master/linux-x64.Compiler.exe
 Rename to Oxide.Compiler.exe

2. Put StackSizeController.cs in: oxide/plugins/
3. Restart the server or: oxide.reload StackSizeController
4. The config will be created in: oxide/config/StackSizeController.json

### Commands (stacksizecontroller.admin permission required)
- stack.set <shortname> <amount>
- stack.setall <amount>
- stack.search <name>
- stack.reload — re-read the config without overwriting

### Access rights
oxide.grant group admin stacksizecontroller.admin

### How to edit the config
1. Open oxide/config/StackSizeController.json
2. Change the desired values
3. Enter: stack.reload
4. The plugin will read the file and apply it — it will NOT overwrite it!

ru:
###Этот плагин НИКОГДА не перезаписывает конфиг при перезагрузке!
Конфиг генерируется ОДИН раз при первом запуске.
После этого плагин ТОЛЬКО читает конфиг.

### Установка
1. Убедитесь что Oxide.Compiler.exe есть в корне сервера
   Если нет — скачайте:
   Windows: https://downloads.oxidemod.com/artifacts/Oxide.Compiler/master/win-x64.Compiler.exe
   Linux: https://downloads.oxidemod.com/artifacts/Oxide.Compiler/master/linux-x64.Compiler.exe
   Переименуйте в Oxide.Compiler.exe

2. Положите StackSizeController.cs в: oxide/plugins/
3. Перезапустите сервер или: oxide.reload StackSizeController
4. Конфиг создастся в: oxide/config/StackSizeController.json

### Команды (нужен пермишн stacksizecontroller.admin)
- stack.set <shortname> <количество>
- stack.setall <количество>
- stack.search <название>
- stack.reload — перечитать конфиг БЕЗ перезаписи

### Права доступа
oxide.grant group admin stacksizecontroller.admin

### Как редактировать конфиг
1. Откройте oxide/config/StackSizeController.json
2. Измените нужные значения
3. Введите: stack.reload
4. Плагин прочитает файл и применит — НЕ перезапишет!


PS: Сообщайте если будет баг.
