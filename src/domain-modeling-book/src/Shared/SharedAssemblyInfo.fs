namespace DomainModelingMadeFunctional
    
    open System.Reflection
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices

    // Disable the warning for the semantic versioning pattern.   
    //
    // NOTE: This is currently broken, but a fix is pending.  
    //       (see: https://github.com/Microsoft/visualfsharp/issues/3139)
    
    #nowarn "2003"
        
    // General Information about an assembly is controlled through the following
    // set of attributes. Change these attribute values to modify the information
    // associated with an assembly.

    [<assembly: AssemblyCompany("Jesse Squire")>]
    [<assembly: AssemblyProduct("Implementation of scenarios from the book 'Domain Modeling Made Functional' by Scott Wlaschin")>]
    [<assembly: AssemblyCopyright("Copyright © Jesse Squire")>]
    [<assembly: AssemblyTrademark("")>]
    [<assembly: AssemblyVersion("1.0.0.0")>]
    [<assembly: AssemblyInformationalVersion("0.0.1-devbuild-local")>]

    // Setting ComVisible to false makes the types in this assembly not visible 
    // to COM components.  If you need to access a type in this assembly from 
    // COM, set the ComVisible attribute to true on that type.

    [<assembly: ComVisible(false)>]

    // Allow internals to be visible to test projects.

    [<assembly: InternalsVisibleTo("DomainModelingMadeFunctional.Orders.Tests")>]

    #if DEBUG
    [<assembly: AssemblyConfiguration("DEBUG")>]
    #else
    [<assembly: AssemblyConfiguration("RELEASE")>]
    #endif

    do ()