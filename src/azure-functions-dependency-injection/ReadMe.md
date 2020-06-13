# Azure Functions Dependency Injection

### Summary

Written in early 2018, before the technique of using input bindings to inject dependencies was widely circulated, this project is an exploration of a pattern to manually manage the infrastructure to create properly scoped dependency injection within Azure Functions. 

The approach taken is to consider the Function entry point as an infrastructure unit with the responsibility of managing DI and other cross-cutting concerns and encapsulating all of the API and business logic in dedicated units elsewhere.  This allows the logic to be tested independent of the Azure Functions host with reasonable support for mocking.

The code herein is prototype-level; best practices are not always adhered to and there is a good level of polish missing for something that I’d consider to be production-worthy.   For example, there are no unit tests, and I’m willfully ignoring best practices around exception handling, logging, and similar considerations.  I did try to comment extensively to narrate what was going on and why, though I did omit some boilerplate XML Doc comments on simple constructs that I felt were self-explanatory.

### Structure

* **src**  
  _The container for project source code._
  
