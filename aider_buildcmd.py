#!/usr/bin/env python3
"""
A simple helper script for building and testing the DotNetRealtimePipeline repository.

Running this script will:

1. Restore NuGet packages.
2. Build the solution in Release configuration.
3. Run all unit tests.

The script is intentionally lightweight and does not require any external
Python packages beyond the standard library.
"""

import subprocess
import sys
import pathlib
from typing import List


def _run_command(command: List[str], cwd: pathlib.Path) -> None:
    """
    Executes a command synchronously, streaming its stdout and stderr.

    Args:
        command: The command and its arguments as a list of strings.
        cwd: The working directory in which to execute the command.

    Raises:
        subprocess.CalledProcessError: If the command exits with a non‑zero status.
    """
    process = subprocess.Popen(
        command,
        cwd=str(cwd),
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
    )

    # Stream output line‑by‑line so the user sees progress in real time.
    assert process.stdout is not None  # for type‑checkers
    for line in process.stdout:
        print(line, end="")

    process.wait()
    if process.returncode != 0:
        raise subprocess.CalledProcessError(process.returncode, command)


def main() -> None:
    # Resolve the repository root (the directory containing this script).
    repo_root = pathlib.Path(__file__).resolve().parent

    try:
        print("\n=== Restoring NuGet packages ===")
        _run_command(["dotnet", "restore"], repo_root)

        print("\n=== Building solution (Release) ===")
        _run_command(["dotnet", "build", "--configuration", "Release"], repo_root)

        print("\n=== Running unit tests ===")
        _run_command(
            ["dotnet", "test", "--no-build", "--configuration", "Release"],
            repo_root,
        )

        print("\nAll steps completed successfully.")
    except subprocess.CalledProcessError as exc:
        print(f"\nCommand failed with exit code {exc.returncode}: {' '.join(exc.cmd)}", file=sys.stderr)
        sys.exit(exc.returncode)
    except FileNotFoundError as exc:
        # This typically means the 'dotnet' CLI is not installed or not on PATH.
        print(f"\nError: {exc}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
