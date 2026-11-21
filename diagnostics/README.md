Diagnostic source and scripts are kept here. Personal save fixtures and generated
outputs are excluded from the repository.

`check-project.py` checks source files and runtime assets using the bundled
upstream source ZIP. `check-updates.ps1` checks update metadata handling.

The save tests (`build-check.ps1`, `test-compatibility.py`, and `compare-save.py`)
depend on the original private `game.decoded.sii` fixture. That fixture is not
distributed. These tests make assertions about its specific contents; an arbitrary
save is not a replacement fixture.
