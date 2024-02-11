# GradsSharp Documentation

GradsSharp is a C# port of the popular [OpenGrads](http://opengrads.org/) Software.
This project is not an executable as OpenGrads is.  Instead this is a .NET library that allows you to write software that automates plotting weather charts and other charts based on gridded data.

## Benefits and Features

* *Library* allowing you to automate plotting from within your own software
* Pluggable data readers:  the library cannot read gridded data, but allows plugging in your own datareader. A sample reader for GFS data is provided
* Graphics output pluggable:  the library does not plot on it's own.  A drawing engine can be implemented by yourself or the standard Cairo engine can be used to output data to image files
* Has mathematics functions included to apply calculations on grids before plotting them


## Getting started

