# GradsSharp Project

This project contains a set of libraries to draw gridded data on maps (one example is weather data).  The code was ported from the [OpenGrads](http://cola.gmu.edu/grads/) project.
Not all functionality was ported (yet) but a basic version is ready to be used.  Expect still some bugs that creeped in during porting from C.


# GradsSharp Library

This library contains the core of the project.  It contains all the code necessary to do the drawing of data, but has a pluggable interface to inject:

* Drawing engines
* Data readers

# GradsSharp.DrawingEngine.Cairo

This library contains a drawing engine implementation as used in the OpenGrads project.   It depends on the c-libraries from the Cairo project to do the drawing to images.