﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeRule.cs" company="Sven Erik Matzen">
//   Copyright (c) Sven Erik Matzen. GNU Library General Public License (LGPL) Version 2.1.
// </copyright>
// <summary>
//   Defines the TypeRule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sem.GenericHelpers.Contracts
{
    using System;

    public class TypeRule
    {
        public object Rule { get; set; }

        public Type ValueType { get; set; }
    }
}