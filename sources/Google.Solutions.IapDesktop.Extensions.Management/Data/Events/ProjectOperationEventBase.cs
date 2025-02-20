﻿//
// Copyright 2019 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Solutions.IapDesktop.Extensions.Management.Data.Logs;
using System.Diagnostics;

namespace Google.Solutions.IapDesktop.Extensions.Management.Data.Events
{
    public abstract class ProjectOperationEventBase : ProjectEventBase
    {
        protected abstract string SuccessMessage { get; }
        protected abstract string ErrorMessage { get; }

        public override string Message => IsError
            ? $"{ErrorMessage} [{this.Status.Message}]" + this.OperationSuffix
            : SuccessMessage + this.OperationSuffix;

        protected string OperationSuffix
        {
            get
            {
                if (this.LogRecord.Operation == null)
                {
                    return string.Empty;
                }
                else if (this.LogRecord.Operation.IsLast)
                {
                    return " (operation completed)";
                }
                else if (this.LogRecord.Operation.IsFirst)
                {
                    return " (operation started)";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsError => this.Severity == "ERROR";

        protected ProjectOperationEventBase(LogRecord logRecord) : base(logRecord)
        {
            Debug.Assert(!IsError || logRecord.ProtoPayload.Status != null);
        }
    }
}