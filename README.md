# Luauni - Unity Branch
This is the branch of Luauni where development goes on in Unity.<br>
The current goal is trying to emulate Robot 64 Engine.

## Emulation Progress
In here progress will be shown on emulating Robot 64 Engine, how far it has gone in the main function and what errored.<br>
`Reached instruction 1433 / 12480`<br>
`Error: ClearAllChildren is not a valid member of mapdebris (UnityEngine.GameObject)`<br>

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
✅ `LOP_GETUPVAL` `LOP_SETUPVAL`<br>
⚠️ `LOP_CLOSEUPVALS`<br>
✅ `LOP_GETIMPORT`<br>
✅ `LOP_GETTABLE` `LOP_SETTABLE`<br>
✅ `LOP_GETTABLEKS`<br>
✅ `LOP_SETTABLEKS`<br>
✅ `LOP_GETTABLEN` `LOP_SETTABLEN`<br>
✅ `LOP_NEWCLOSURE`<br>
✅ `LOP_NAMECALL`<br>
✅ `LOP_CALL`<br>
✅ `LOP_RETURN`<br>
✅ `LOP_JUMP` `LOP_JUMPBACK`<br>
✅ `LOP_JUMPIF` `LOP_JUMPIFNOT` `LOP_JUMPIFEQ` `LOP_JUMPIFLT` `LOP_JUMPIFNOTEQ` `LOP_JUMPIFNOTLT`<br>
❌ `LOP_JUMPIFLE` `LOP_JUMPIFNOTLE`<br>
✅ `LOP_ADD` `LOP_SUB` `LOP_MUL` `LOP_DIV` `LOP_MOD` `LOP_POW`<br>
✅ `LOP_ADDK` `LOP_SUBK` `LOP_MULK` `LOP_DIVK` `LOP_MODK` `LOP_POWK`<br>
✅ `LOP_AND` `LOP_OR`<br>
❌ `LOP_ANDK` `LOP_ORK`<br>
✅ `LOP_CONCAT`<br>
✅ `LOP_MINUS`<br>
✅ `LOP_NOT` `LOP_LENGTH`<br>
✅ `LOP_NEWTABLE`<br>
✅ `LOP_DUPTABLE`<br>
✅ `LOP_SETLIST`<br>
✅ `LOP_FORNPREP` `LOP_FORNLOOP`<br>
⚠️ `LOP_FORGPREP` `LOP_FORGLOOP`<br>
❌ `LOP_FORGLOOP_INEXT` `LOP_DEP_FORGLOOP_INEXT`<br>
⚠️ `LOP_FORGPREP_NEXT`<br>
❌ `LOP_NATIVECALL`<br>
⚠️ `LOP_GETVARARGS` `LOP_PREPVARARGS`<br>
✅ `LOP_DUPCLOSURE`<br>
❌ `LOP_LOADKX`<br>
❌ `LOP_JUMPX`<br>
⚠️ `LOP_FASTCALL` `LOP_FASTCALL1` `LOP_FASTCALL2` `LOP_FASTCALL2K`<br>
❌ `LOP_COVERAGE`<br>
✅ `LOP_CAPTURE`<br>
❌ `LOP_SUBRK` `LOP_DIVRK`<br>
✅ `LOP_JUMPXEQKNIL` `LOP_JUMPXEQKB` `LOP_JUMPXEQKN` `LOP_JUMPXEQKS`<br>
❌ `LOP_IDIV` `LOP_IDIVK`<br>
