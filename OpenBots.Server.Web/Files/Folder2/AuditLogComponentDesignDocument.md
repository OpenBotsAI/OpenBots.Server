Author: Nicole Carrero
Creation Date: 8/4/2020

Updated On: 1/7/2021
Updated By: Nicole Carrero

**Audit Log Component**

**Context**

- Problem: Administrators cannot see updates and modifications that have been made to the Server.  An administrator should be able to view audit logs pertaining to actions (add, update, delete, send email, login, etc.) taken on the Server. An administrator should be able to filter a list of audit logs by service name and view more details about an audit log.  An administrator should be able to export a list of audit logs into a file.
- Requirements: Ensure that the administrator is able to view the audit logs for the system, filter those logs, and view more details about each audit log including the pre/post modification values.  Ensure that the administrator is able to export the list of audit logs to a file.

**Component Scope**

- In-Scope:
  - Administrator can monitor all audit logs stored by the Server.
  - Administrator can monitor detailed information of each audit log.
  - Administrator can filter the list of audit logs by service name.
  - Administrator can export a file listing the audit logs to the local computer.
- Out-of-Scope:
  - Administrator and/or user can sort audit logs by created on, created by, method name, parameter(s), exception, timestamp, changed from value, and changed to value.
  - Administrator and/or user can add, edit, or delete audit logs and/or audit log details.

**Design**

- Overview: The overall structure uses the AuditLogsController to allow the user to make a web request for either the entire list of audit logs or the details of an individual audit log.  If the user makes a request, the web request goes to the AuditLogsController, which in turn may call the AuditLogManager, and then calls the AuditLogsRepository for access to the appropriate data from the Server.  If the request is valid, the data will be sent back to the user in the view.  If the request is invalid, the user will receive an error message stating that the audit logs could not be found.
- Proposed Solution:
  - User Interface:
    - In the Dashboard, there will be a link to redirect the user to the list of audit logs.
    - The audit logs will be displayed in a table with the most recent audit log at the top.
      - The headers will be: Service Name, Method Name, Created By, Created On and View.
        - The appropriate data will be displayed underneath each heading for all audit logs.
        - The user can click on each audit log, where the user will be redirected to a view of that specific audit log's details based on its id.
      - There will be a feature to filter audit logs by service name.  There will be a way to go back to view all audit logs.
      - There will be a button to export the audit logs to a CSV file.
    - On the page showing individual audit log details, all appropriate information will be displayed about each audit log, including: Id, Is Deleted, Delete On, Deleted By, Created By, Created On, Timestamp, Service Name, Method Name, Parameter(s), Exception(s), Updated On, Updated By, Changed From, and Changed To.
      - There will be an option to go back to the view of of all audit logs.
  - AuditLogs Controller:
    - The AuditLogsController will make an API request to the AuditLogsRepository to retrieve audit logs from the Server and will return that information back to the view.  The user can view all audit logs or individual audit log details as well as export a file of a list of audit logs.
    - Routes:
      - All audit logs: [HttpGet("api/v1/auditlogs")]
        - Payloads
          - Input : None
          - Output : JSON file listing audit log information
      - All audit logs (view model): [HttpGet("api/v1/auditlogs/view")]
        - Payloads
          - Input : None
          - Output : JSON file listing audit log view model information
      - Count audit logs: [HttpGet("api/v1/auditlogs/count")]
        - Payloads
          - Input : None
          - Output : Number of audit logs in Server
     - Individual audit log details: [HttpGet("api/v1/auditlogs/{id}")]
       - Payloads
         - Input : Audit log id
         - Output : JSON file listing the audit log details for a specific audit log
    - Individual audit log details: [HttpGet("api/v1/auditlogs/{id}/view")]
      - Payloads
        - Input : Audit log id
        - Output : JSON file listing the audit log view model details for a specific audit log.
     - Audit logs lookup: [HttpGet("api/v1/auditlogs/auditlogslookup")]
       - Payloads
         - Input : None
         - Output : List of Service Names
     - Export audit logs: [HttpGet("api/v1/auditlogs/export")]
       - Payloads
         - Input : None
         - Output : CSV/JSON/zip file listing audit log information
  - AuditLog Manager:
    - - The AuditLogManager will inherit IAuditLogManager and BaseManager, which inherits IManager.
      - Beyond the base class and interfaces, AuditLogManager will implement the GetAuditLogs() and ZipCsv() methods to assist AuditLogsController.
  - AuditLog Repository:
    - The AuditLogRepository will retrieve all audit logs or the details of an individual audit log.  It will utilize the GetServiceName() and FindAllView() methods to assist the AuditLogsController and AuditLogManager.
- Model / View Models:
  - AuditLog Data Model:
    - The AuditLog data model will be used to view details of each audit log.  It will inherit Entity and INonAuditable.
      - Beyond the base class and interface, AuditLog will have Guid ObjectId, string ServiceName, string MethodName, string ParametersJson, string ExceptionJson, string ChangedFromJson, and string ChangedToJson.
  - AuditLogDetails View Model
    - The AuditLogDetailsViewModel view model will be used to view details of each audit log.  It will inherit Entity and IViewModel<AuditLog, AuditLogDetailsViewModel>.
      - Beyond the base class and interface, AuditLogDetailsViewModel will have Guid ObjectId, string ServiceName, string MethodName, string ParametersJson, string ExceptionJson, string ChangedFromJson, and string ChangedToJson.
  - AuditLogsLookup View Model
    - The AuditLogsLookupViewModel view model will be used to view the list of service names.
      - It will have List<string> ServiceNameList>.
  - AuditLog View Model
    - The AuditLogViewModel view model will be used to view details of all audit logs.  It will inherit IViewModel<AuditLog, AuditLogViewModel>.
      - Beyond the interface, AuditLogViewModel will have string ServiceName, string MethodName, string CreatedBy, DateTime CreatedOn, Guid ObjectId, and Guid Id.

**Sequence Diagrams**
- [Audit Log Component Sequence Diagram](Sequence Diagrams/AuditLogComponentSequenceDiagram.png)
- [Count Audit Logs Sequence Diagram](Sequence Diagrams/CountAuditLogsSequenceDiagram.png)
- [Audit Log Details Sequence Diagram](Sequence Diagrams/AuditLogDetailsSequenceDiagram.png)
- [Audit Logs Lookup Sequence Diagram](Sequence Diagrams/AuditLogsLookupSequenceDiagram.png)
- [Export Audit Logs Sequence Diagram](Sequence Diagrams/ExportAuditLogsSequenceDiagram.png)

**Unit Tests**

- Positive Test Cases: N/A
- Negative Test Cases: N/A