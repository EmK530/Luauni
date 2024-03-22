# Luauni
A Luau bytecode interpreter programmed with the intention of being used for a Unity port of a Roblox game.

## Requirements
<b>luau-compile.exe with Luau Version 5 and Type Version 1</b><br>
This is what Luauni is being designed for and while it allows other versions, it may cause issues.

## Note
This is the new remake of Luauni where I plan to make better execution now that I know what is required for it to work in Unity.<br>
Opcode support is even worse here because I just started.<br><br>
<b>When compiling a script for Luauni, please compile with -O0 and -g0</b><br>
This is important because optimization and debug levels use some complicated opcodes that may not be supported ever.

## Opcode Support
This is a list of all the opcodes Luauni currently supports.<br>
If there is a ⚠️ then Luauni either ignores it or the implementation is assumed to not be fully correct/done.<br>
If there is an ❌ then if it is encountered Luauni will stop execution.<br>
⚠️ `LOP_NOP`<br>
⚠️ `LOP_BREAK`<br>
✅ `LOP_LOADNIL`<br>
✅ `LOP_LOADB` `LOP_LOADN` `LOP_LOADK`<br>
✅ `LOP_MOVE`<br>
✅ `LOP_GETGLOBAL` `LOP_SETGLOBAL`<br>
❌ `LOP_GETUPVAL` `LOP_SETUPVAL` `LOP_CLOSEUPVALS`<br>
❌ `LOP_GETIMPORT`<br>
✅ `LOP_GETTABLE` `LOP_SETTABLE`<br>
✅ `LOP_GETTABLEKS`<br>
❌ `LOP_SETTABLEKS` `LOP_GETTABLEN` `LOP_SETTABLEN`<br>
✅ `LOP_NEWCLOSURE`<br>
❌ `LOP_NAMECALL`<br>
✅ `LOP_CALL`<br>
✅ `LOP_RETURN`<br>
✅ `LOP_JUMP` `LOP_JUMPBACK`<br>
✅ `LOP_JUMPIF` `LOP_JUMPIFNOT` `LOP_JUMPIFEQ` `LOP_JUMPIFLT` `LOP_JUMPIFNOTEQ` `LOP_JUMPIFNOTLT`<br>
❌ `LOP_JUMPIFLE` `LOP_JUMPIFNOTLE`<br>
✅ `LOP_ADD` `LOP_SUB` `LOP_MUL` `LOP_DIV` `LOP_MOD` `LOP_POW`<br>
❌ `LOP_ADDK` `LOP_SUBK` `LOP_MULK` `LOP_DIVK` `LOP_MODK` `LOP_POWK`<br>
✅ `LOP_AND` `LOP_OR`<br>
❌ `LOP_ANDK` `LOP_ORK`<br>
✅ `LOP_CONCAT`<br>
❌ `LOP_MINUS`<br>
✅ `LOP_NOT` `LOP_LENGTH`<br>
✅ `LOP_NEWTABLE`<br>
✅ `LOP_DUPTABLE`<br>
✅ `LOP_SETLIST`<br>
✅ `LOP_FORNPREP` `LOP_FORNLOOP`<br>
⚠️ `LOP_FORGPREP` `LOP_FORGLOOP`<br>
❌ `LOP_FORGLOOP_INEXT` `LOP_DEP_FORGLOOP_INEXT`<br>
❌ `LOP_FORGPREP_NEXT`<br>
❌ `LOP_NATIVECALL`<br>
⚠️ `LOP_GETVARARGS` `LOP_PREPVARARGS`<br>
❌ `LOP_DUPCLOSURE`<br>
❌ `LOP_LOADKX`<br>
❌ `LOP_JUMPX`<br>
❌ `LOP_FASTCALL` `LOP_FASTCALL1` `LOP_FASTCALL2` `LOP_FASTCALL2K`<br>
❌ `LOP_COVERAGE`<br>
❌ `LOP_CAPTURE`<br>
❌ `LOP_SUBRK` `LOP_DIVRK`<br>
❌ `LOP_JUMPXEQKNIL` `LOP_JUMPXEQKB` `LOP_JUMPXEQKN` `LOP_JUMPXEQKS`<br>
❌ `LOP_IDIV` `LOP_IDIVK`<br>

## Implemented Globals
<b>`math`</b> - 17.6%<br>
### Topmost Functions
`print`<br>
`warn`<br>
`tostring`<br>
`tonumber`<br>
`pairs`<br>
