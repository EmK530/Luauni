# Luauni
A Luau bytecode interpreter programmed with the intention of being used for a Unity port of a Roblox game.

## Note
Luauni is still in development and has limited support, work is being done!

## Support
This is a list of all the opcodes Luauni currently supports.<br>
If there is a warning icon then it's either implemented inaccurately or unstable.<br>
❌ `LOP_NOP`<br>
❌ `LOP_BREAK`<br>
✅ `LOP_LOADNIL`<br>
✅ `LOP_LOADB` `LOP_LOADN` `LOP_LOADK`<br>
✅ `LOP_MOVE`<br>
✅ `LOP_GETGLOBAL` `LOP_SETGLOBAL`<br>
❌ `LOP_GETUPVAL` `LOP_SETUPVAL` `LOP_CLOSEUPVALS`<br>
❌ `LOP_GETIMPORT`<br>
⚠️ `LOP_GETTABLE`<br>
❌ `LOP_SETTABLE` `LOP_GETTABLEKS` `LOP_SETTABLEKS` `LOP_GETTABLEN` `LOP_SETTABLEN`<br>
⚠️ `LOP_NEWCLOSURE`<br>
❌ `LOP_NAMECALL`<br>
✅ `LOP_CALL`<br>
✅ `LOP_RETURN`<br>
✅ `LOP_JUMP` `LOP_JUMPBACK`<br>
✅ `LOP_JUMPIF` `LOP_JUMPIFNOT` `LOP_JUMPIFEQ` `LOP_JUMPIFLT` `LOP_JUMPIFNOTEQ` `LOP_JUMPIFNOTLT`<br>
❌ `LOP_JUMPIFLE` `LOP_JUMPIFNOTLE`<br>
✅ `LOP_ADD` `LOP_SUB` `LOP_MUL` `LOP_DIV`<br>
❌ `LOP_MOD` `LOP_POW`<br>
❌ `LOP_ADDK` `LOP_SUBK` `LOP_MULK` `LOP_DIVK` `LOP_MODK` `LOP_POWK`<br>
❌ `LOP_AND` `LOP_OR`<br>
❌ `LOP_ANDK` `LOP_ORK`<br>
✅ `LOP_CONCAT`<br>
❌ `LOP_NOT` `LOP_MINUS` `LOP_LENGTH`<br>
⚠️ `LOP_NEWTABLE`<br>
❌ `LOP_DUPTABLE`<br>
⚠️ `LOP_SETLIST`<br>
✅ `LOP_FORNPREP` `LOP_FORNLOOP`<br>
❌ `LOP_FORGPREP` `LOP_FORGLOOP`<br>
❌ `LOP_FORGLOOP_INEXT` `LOP_DEP_FORGLOOP_INEXT`<br>
❌ `LOP_FORGPREP_NEXT`<br>
❌ `LOP_NATIVECALL`<br>
❌ `LOP_GETVARARGS` `LOP_PREPVARARGS`<br>
❌ `LOP_DUPCLOSURE`<br>
❌ `LOP_LOADKX`<br>
❌ `LOP_JUMPX`<br>
❌ `LOP_FASTCALL` `LOP_FASTCALL1` `LOP_FASTCALL2` `LOP_FASTCALL2K`<br>
❌ `LOP_COVERAGE`<br>
❌ `LOP_CAPTURE`<br>
❌ `LOP_SUBRK` `LOP_DIVRK`<br>
❌ `LOP_JUMPXEQKNIL` `LOP_JUMPXEQKB` `LOP_JUMPXEQKN` `LOP_JUMPXEQKS`<br>
❌ `LOP_IDIV` `LOP_IDIVK`<br>
