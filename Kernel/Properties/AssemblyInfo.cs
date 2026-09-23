///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true)]

// ============================================================
// Основная информация о сборке
// ============================================================

// Краткое название библиотеки. Отображается в заголовках окон, 
// диалогах свойств файла и т.д.
[assembly: AssemblyTitle("Kernel")]

// Подробное описание назначения библиотеки. 
// Видно в свойствах файла → вкладка "Подробно".
[assembly: AssemblyDescription("Декларативная система компоновки UI")]

// Конфигурация сборки (Debug/Release)
[assembly: AssemblyConfiguration("")]

// Название компании или организации-разработчика
[assembly: AssemblyCompany("AES")]

// Название продукта, частью которого является сборка
[assembly: AssemblyProduct("DisplayNodes")]

// Копирайт
[assembly: AssemblyCopyright("Copyright 2026 AES")]

// Торговая марка
[assembly: AssemblyTrademark("")]

// Культура сборки
[assembly: AssemblyCulture("")]

// ============================================================
// COM-видимость
// ============================================================

// false = сборка не видна из COM-клиентов
// true = только если библиотека должна использоваться из VB6, VBA, Classic COM.
[assembly: ComVisible(false)]

// Уникальный идентификатор библиотеки для COM. 

[assembly: Guid("aacc9e12-a41a-48df-80bc-c4b528a46962")]

// ============================================================
// Версионирование
// ============================================================

// AssemblyVersion — версия, используемая CLR для привязки сборок.
// Формат: Major.Minor.Build.Revision
// 
// Правила:
// - Major: меняется при несовместимых API-изменениях (breaking changes)
// - Minor: меняется при добавлении функционала обратно-совместимо
// - Build: номер сборки (можно автоинкрементировать через *)
// - Revision: hotfix/патч (можно автоинкрементировать через *)
// 
// Примеры:
// "1.0.0.0"      — стабильный релиз
// "1.0.*"        — автоинкремент Build и Revision
// "1.2.3.4"      — явное указание всех частей

[assembly: AssemblyVersion("1.0.0.0")]

// AssemblyFileVersion — версия файла, видимая в проводнике Windows 
// (свойства файла → вкладка "Подробно").
// Обычно совпадает с AssemblyVersion, но может отличаться для патчей.

[assembly: AssemblyFileVersion("1.0.0.0")]

// AssemblyInformationalVersion — семантическая версия для пользователей.
// Поддерживает суффиксы: "1.0.0-beta", "1.2.3-rc1", "2.0.0-alpha.1"
// Видна в NuGet-пакетах и диалогах "О программе".

[assembly: AssemblyInformationalVersion("1.0.0")]