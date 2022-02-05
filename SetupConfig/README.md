# Build Windows Setup Executable for SeedCalc

[SeedCalcWinSetup.iss](./SeedCalcWinSetup.iss) is the [Inno
Setup](https://jrsoftware.org/isinfo.php) config script to build Windows setup
executable for SeedCalc.

How to build:

1. Build SeedCalc for Windows with Unity. The output of this step is a folder
   that contains the EXEs, DLLs and data assets of SeedCalc. Name the folder
   `SeedCalcWin` and put it together with `SeedCalcWinSetup.iss`.

2. Open the script `SeedCalcWinSetup.iss` with Inno Setup.

3. Update `SeedCalcVersion` in `SeedCalcWinSetup.iss`.

4. Compile the script with Inno Setup.
