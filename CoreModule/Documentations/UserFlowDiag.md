```mermaid

flowchart TD
    A[User Login] --> B{Is User Verified?}
    B -- No --> B1[Show Error Message]
    B -- Yes --> C[Fetch Allowed Businesses]
    C --> D[Select Business]

    D --> E{Is User Owner or Approved Access?}
    E -- No --> E1[Request Access from Owner]
    E -- Yes --> F[Check Subscription Limits]

    F --> F1{Business Active?}
    F1 -- No --> F2[Prompt for Renewal]
    F1 -- Yes --> G[Check Company & User Limits]

    G --> G1{Within Allowed Limits?}
    G1 -- No --> G2[Restrict Access / Show Warning]
    G1 -- Yes --> H[Load Accessible Modules]

    H --> I[User Selects Accounts Module]
    I --> J[Load Accounts Module UI]
    J --> K[Fetch Accounts Data]
