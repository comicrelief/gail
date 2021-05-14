# Gift Aid Interface Link (GAIL)

This is a library for adding to .Net applications to handle the creation and processing of files for the Government's Charities Online service, in order to make claims for Gift Aid.

# What is GAIL?

GAIL is a Windows class library which provides a set of methods for:
-	Creating Government Gateway submission files 
-	Submitting files to the Government Gateway 
-	Reading and responding to messages from the Government Gateway.

It consists of:
-	HMRC service classes – a representation of the government XML schema in object oriented form
-	Two classes which create and read files
-	A dispatcher class which handles sending files
-	A controller class which allows you to control the above

Environment needed to run it:
-	Windows
-	Any version that has .Net framework 4.0 or higher installed

To use the tool you would normally call controller methods to undertake whatever action you want to perform.  So, for example, the simplest thing would be to create a submission file from a data set.

# Installation

Git clone the repo. Build in Visual Studio version 12 or higher.

# Getting Started

A Demonstration harness is provided in `CR.CO.Demo` which is a console application that can be used to play around with the library, creating files and reading responses. This is a good place to start in learning how to use this application.

If running in Debug mode, remember to check that the Solution startup project is set to `CR.CO.Demo`.

# Documentation

See the [Technical Guidance](Documents/technical_guidance.md) for detailed instructions on use.

