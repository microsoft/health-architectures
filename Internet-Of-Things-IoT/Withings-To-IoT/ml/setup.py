# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from pathlib import Path

from setuptools import find_packages, setup

src_dir = Path(__file__).parent

version = (src_dir / "version.txt").read_text(encoding="utf-8")

setup(
    name="h3_utils",
    version=version,
    python_requires='>=3.6',
    setup_requires=[
        "pip>=20.2.2",
        "setuptools>=49.6.0",
        "wheel>=0.35.1",
    ],
    packages=find_packages(include=[
        "h3_utils",
        "h3_utils.*",
    ]),
)
