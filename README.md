# KeywordDriven

Generic test automation framework based on the **keyword-driven testing** approach. Test cases are authored as rows in an Excel workbook: each step names a keyword, an optional page object, and test data. The `Keywords` class exposes the public static methods that the driver invokes by keyword name.

## Architecture

The framework separates test data, execution, browser keywords, and reporting:

```text
NUnit test runner (KeywordDriven.TestRunner)
        |
        | initializes workbook, browser settings, logs, and report
        v
DriverScript (Execution)
        |
        | reads enabled test cases and ordered steps from Excel
        | invokes the named public Keywords method
        v
Keywords (ActionKeywords)
        |
        +--> Drivers   --> Selenium WebDriver and browser
        +--> Locators  --> page-object key lookup and element actions
        +--> Waits     --> explicit waits for elements, URLs, and page load
        +--> Assertions / Verifications --> checks and step outcomes
        |
        v
Excel results + execution logs + Extent HTML report
```

| Component | Responsibility |
|---|---|
| `KeywordDriven.TestRunner` | NUnit entry point. Sets up the test workbook, driver settings, logger, and report, then calls the driver script. |
| `KeywordDriven.UnitTests` | NUnit project for isolated framework tests. It checks the public keyword dispatch contract and unit-level behavior of drivers, locators, waits, assertions, and soft verifications using mocked Selenium interfaces. |
| `Execution/DriverScript` | Reads test cases and steps, skips cases not marked `Yes`, invokes the keyword named by each step, and records step and case results. |
| `ActionKeywords/Keywords` | Public keyword API split across `Drivers.cs`, `Locators.cs`, `Waits.cs`, `Assertions.cs`, and `Verifications.cs`. The driver resolves each method by name and passes the step's page object and test data. |
| `ActionKeywords/Drivers` | Creates and controls the Selenium WebDriver and browser lifecycle, navigation, windows, and frames. |
| `ActionKeywords/Locators` | Resolves page-object keys from the `Locators` worksheet and performs element interactions. |
| `ActionKeywords/Waits` | Provides explicit waits used directly by wait steps and by actions that need an element to reach a state. |
| `ActionKeywords/Assertions` and `Verifications` | Evaluate element and page conditions. Assertions fail a step immediately; soft verifications collect failures while the remaining steps continue. |
| `Utils/ExcelManager` | Opens the workbook, reads test definitions and locators, and writes step and case results. |
| `Utils/Logger`, `ExtentReporter`, and `ScreenShots` | Support execution diagnostics and test reporting. The NUnit teardown flushes the HTML report and saves/closes the workbook. |
| `Config` | Holds workbook sheet/column definitions, output paths, and browser/timeout settings. |

For each enabled case, `DriverScript` reads its steps in workbook order. A successful keyword marks the step as passed; a failed assertion or execution error marks the step and stops that case. Failed soft verifications are summarized after the steps finish and cause the case to fail without stopping subsequent steps.

## Running unit tests

Run the unit test project independently from the browser-based automation suite:

```powershell
dotnet test KeywordDriven.UnitTests\KeywordDriven.UnitTests.csproj
```

The tests exercise framework logic without starting a real browser; the Excel-driven browser flow remains in `KeywordDriven.TestRunner`.

## Writing test steps

The default workbook is `TestCases.xlsx` and uses these worksheets:

| Worksheet | Purpose |
|---|---|
| `TestCases` | Test case ID, title, description, run mode (`Yes` or `No`), and result |
| `TestSteps` | Test case ID, step number, description, page object, action keyword, test data, and result |
| `Locators` | Page object key and its locator value |
| `Settings` | Framework settings |

`TestSteps` columns are read in this order: `TestCaseID`, `StepNo`, `Description`, `PageObject`, `ActionKeyword`, `TestData`, `Result`.

Every public keyword has the signature `static void KeywordName(string obj, string data)`. The driver supplies:

- **`obj`**: a key from the `Locators` worksheet for element actions, waits, and element checks. A key must include an underscore followed by a locator type, for example `LoginButton_id` or `SearchBox_css`. The key's matching row in `Locators` supplies the locator value. Page-level and browser keywords generally leave this blank.
- **`data`**: the keyword's value, such as text to enter, a URL, an expected value, or a duration. Keywords that do not need a value ignore it.

Example test steps (the `Locators` sheet must contain the referenced page object keys):

| TestCaseID | StepNo | Description | PageObject | ActionKeyword | TestData |
|---|---:|---|---|---|---|
| TC01 | 1 | Open Chrome | | `OpenBrowser` | `Chrome` |
| TC01 | 2 | Open the login page | | `NavigateToUrl` | `https://example.com/login` |
| TC01 | 3 | Enter the user name | `UserName_id` | `TypeText` | `sample-user` |
| TC01 | 4 | Submit the form | `LoginButton_id` | `Click` | |
| TC01 | 5 | Check the page title | | `VerifyPageHaveTitle` | `Welcome` |

## Keyword catalog

### Browser and navigation

| Keyword | Purpose / `data` |
|---|---|
| `OpenBrowser` | Start the requested browser; `data` specifies the browser. |
| `CloseBrowser` | Close the current browser window. |
| `CloseAllBrowser` | Close all browser windows. |
| `MaximizeBrowser`, `MinimizeBrowser`, `FullScreenBrowser` | Change the current browser window display mode. |
| `SetWindowSize` | Set the window dimensions; provide `width,height`, for example `1280,720`. |
| `NavigateToUrl` | Navigate to the URL in `data`. |
| `RefreshPage`, `NavigateBack`, `NavigateForward` | Refresh the page or navigate through browser history. |
| `SwitchToWindow` | Switch to the window identified by `data`. |
| `SwitchToFrame` | Switch to the frame identified by page object `obj`. |
| `SwitchToDefaultContent` | Return from a frame to the top-level page. |

### Element actions

| Keyword | Purpose / `data` |
|---|---|
| `Click`, `ClickByActions`, `DoubleClick`, `RightClick` | Click, action-chain click, double-click, or right-click the element in `obj`. |
| `TypeText` | Replace the element's text with `data`. |
| `AppendText` | Append `data` to the element's current text. |
| `ClearText` | Clear the element's text. |
| `PressKey` | Send the key named in `data` to the element. |
| `CheckCheckbox`, `UnCheckCheckbox` | Check or uncheck a checkbox. |
| `SelectRadioButton` | Select the radio button located by `obj`. |
| `SelectRadioButtonByText`, `SelectRadioButtonByValue`, `SelectRadioButtonByIndex` | Select a radio option by its text, value, or zero-based index in `data`. |
| `SelectByText`, `SelectByValue`, `SelectByIndex` | Select an option in a select element by text, value, or zero-based index in `data`. |
| `SelectFirstOption`, `SelectLastOption` | Select the first or last option in a select element. |
| `SelectMultipleByText` | Select multiple options by text; separate values in `data` with `|`. |
| `DeselectAll` | Deselect all options in a multi-select element. |
| `UploadFile` | Upload the file at the path in `data`. |
| `UploadMultipleFiles` | Upload multiple files; separate paths in `data` with `|` or `,`. |

### Waits

Element waits use `obj` to identify the page object. URL, page-load, and duration waits use `data`.

| Keyword | Waits until |
|---|---|
| `WaitUntilClickable` | The element can be clicked. |
| `WaitUntilExists`, `WaitUntilNotExists` | The element exists or no longer exists. |
| `WaitUntilVisible`, `WaitUntilNotVisible` | The element is visible or no longer visible. |
| `WaitUntilAllVisible`, `WaitUntilAllNotVisible` | All matching elements are visible or all are not visible. |
| `WaitUntilExistsAndVisible` | The element exists and is visible. |
| `WaitUntilEditable`, `WaitUntilNotEditable` | The element is editable or is no longer editable. |
| `WaitUntilTextContains` | The element's text contains `data`. |
| `WaitUntilUrlContains`, `WaitUntilUrlContainsIgnoreCase` | The current URL contains `data`, with or without case-sensitive matching. |
| `WaitUntilUrlEquals` | The current URL equals `data`. |
| `WaitUntilUrlStartsWith`, `WaitUntilUrlEndsWith` | The current URL starts or ends with `data`. |
| `WaitUntilUrlMatchesPattern` | The current URL matches the regular expression in `data`. |
| `WaitUntilPageLoaded` | The page document has loaded. |
| `WaitSeconds` | The number of seconds in `data` has elapsed. |

### Assertions and verifications

Both keyword families check the same kinds of conditions. **Assertions** (`Assert...`) fail the current test step on a failed condition. **Verifications** (`Verify...`) record a soft failure so later steps in the test case can continue; accumulated verification failures make the test case fail at the end.

Element keywords use `obj` to identify the element. For text and attribute comparisons, provide the expected text or attribute name in `data`. For `...HaveAriaRole` / `...NotHaveAriaRole`, `data` is the role name.

| Assertion keyword | Verification keyword | Condition |
|---|---|---|
| `AssertElementExists`, `AssertElementNotExists` | `VerifyElementExists`, `VerifyElementNotExists` | Element exists or does not exist. |
| `AssertElementVisible`, `AssertElementNotVisible` | `VerifyElementVisible`, `VerifyElementNotVisible` | Element is visible or not visible. |
| `AssertElementEditable`, `AssertElementNotEditable` | `VerifyElementEditable`, `VerifyElementNotEditable` | Element is editable or not editable. |
| `AssertElementEnabled`, `AssertElementDisabled` | `VerifyElementEnabled`, `VerifyElementDisabled` | Element is enabled or disabled. |
| `AssertElementFocused` | `VerifyElementFocused` | Element currently has focus. |
| `AssertElementHaveAttribute`, `AssertElementNotHaveAttribute` | `VerifyElementHaveAttribute`, `VerifyElementNotHaveAttribute` | Element has or does not have the attribute named in `data`. |
| `AssertElementHaveAriaRole`, `AssertElementNotHaveAriaRole` | `VerifyElementHaveAriaRole`, `VerifyElementNotHaveAriaRole` | Element's ARIA role equals or differs from `data`. |
| `AssertElementContainsText`, `AssertElementNotContainsText` | `VerifyElementContainsText`, `VerifyElementNotContainsText` | Element text contains or does not contain `data`. |
| `AssertElementHaveText`, `AssertElementNotHaveText` | `VerifyElementHaveText`, `VerifyElementNotHaveText` | Element text equals or differs from `data`. |
| `AssertElementTextStartsWith`, `AssertElementTextEndsWith` | `VerifyElementTextStartsWith`, `VerifyElementTextEndsWith` | Element text starts or ends with `data`. |
| `AssertElementTextMatchesPattern` | `VerifyElementTextMatchesPattern` | Element text matches the regular expression in `data`. |
| `AssertSelectedText`, `AssertSelectedValue`, `AssertSelectedIndex` | `VerifySelectedText`, `VerifySelectedValue`, `VerifySelectedIndex` | Selected option's text, value, or zero-based index matches `data`. |
| `AssertCheckboxChecked`, `AssertCheckboxNotChecked` | `VerifyCheckboxChecked`, `VerifyCheckboxNotChecked` | Checkbox is checked or unchecked. |
| `AssertRadioSelected`, `AssertRadioNotSelected` | `VerifyRadioSelected`, `VerifyRadioNotSelected` | Radio button is selected or not selected. |
| `AssertPageContainsTitle`, `AssertPageNotContainsTitle` | `VerifyPageContainsTitle`, `VerifyPageNotContainsTitle` | Page title contains or does not contain `data`. |
| `AssertPageHaveTitle`, `AssertPageNotHaveTitle` | `VerifyPageHaveTitle`, `VerifyPageNotHaveTitle` | Page title equals or differs from `data`. |
| `AssertPageContainsUrl`, `AssertPageNotContainsUrl` | `VerifyPageContainsUrl`, `VerifyPageNotContainsUrl` | Current URL contains `data`; the negative checks that it does not. |
| `AssertPageHaveUrl` | `VerifyPageHaveUrl` | Current URL equals `data`. |
| `AssertPageUrlStartsWith`, `AssertPageUrlEndsWith` | `VerifyPageUrlStartsWith`, `VerifyPageUrlEndsWith` | Current URL starts or ends with `data`. |
| `AssertPageUrlMatchesPattern` | `VerifyPageUrlMatchesPattern` | Current URL matches the regular expression in `data`. |
