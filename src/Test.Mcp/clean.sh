#!/bin/bash

rm -rf *.log

if [ -d "bin" ]; then rm -rf bin; fi
if [ -d "obj" ]; then rm -rf obj; fi
