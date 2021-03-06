// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   GlobalSuppressions.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", 
        Scope = "member", Target = "Sem.Sync.Connector.MeinVZ.ContactClient.#HttpUrlBaseAddress")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", 
        Scope = "member", Target = "Sem.Sync.Connector.MeinVZ.ContactClient.#HttpUrlLogOnRequest")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", 
        Scope = "member", 
        Target =
            "Sem.Sync.Connector.MeinVZ.ContactClient.#ReadFullList(System.String,System.Collections.Generic.List`1<Sem.Sync.SyncBase.StdElement>)"
        )]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", 
        Scope = "member", 
        Target =
            "Sem.Sync.Connector.MeinVZ.ContactClient.#WriteFullList(System.Collections.Generic.List`1<Sem.Sync.SyncBase.StdElement>,System.String,System.Boolean)"
        )]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", 
        Scope = "member", Target = "Sem.Sync.Connector.MeinVZ.ContactClient.#DownloadContact(System.String)")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", 
        Scope = "member", 
        Target =
            "Sem.Sync.Connector.MeinVZ.ContactClient.#MapKeyValuePair(Sem.Sync.SyncBase.StdContact,System.String,System.String)"
        )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "Sem.Sync.Connector.MeinVZ.ContactClient.#ReadFullList(System.String,System.Collections.Generic.List`1<Sem.Sync.SyncBase.DetailData.StdElement>)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "Sem.Sync.Connector.MeinVZ.ContactClient.#MapKeyValuePair(Sem.Sync.SyncBase.DetailData.StdContact,System.String,System.String)")]
