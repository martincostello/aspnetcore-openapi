// Copyright (c) Martin Costello, 2024. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp;

// Create the default web application builder
var builder = WebApplication.CreateBuilder(args);

// Add TodoApp into the web application builder
builder.AddTodoApp();

// Create the app
var app = builder.Build();

// Use TodoApp middleware and endpoints with the web application
app.UseTodoApp();

// Run the application
app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}
