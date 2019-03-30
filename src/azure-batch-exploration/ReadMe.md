# Azure Batch Exploration #

### Summary ###

Some exploration of the Azure Batch service for some basic scenarios for processing via an on-demand pool of machines, with explicit control over task creation and monitoring.

The code is prototype-level, which is to say that there’s a bunch of best practices and polish missing for something that I’d consider to be production-worthy.   For example, there are no unit tests, much was left static and local to the entry point, and I’m willfully ignoring best practices around exception handling.  On the upside, I did try to comment extensively to narrate what was going on and why.   There remains a ton of depth to Azure Batch that I haven’t explored, much of which is dedicated to finer-grained control over task and pool management and capturing of task output.

### Structure ###

* **src**  
_The container for project source code._
