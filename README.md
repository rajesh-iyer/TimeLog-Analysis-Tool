Timelog analysis tool
=====================
This is a set of commandline utilities that help scrum teammembers track progress of their tasks in any requirement management system (ofcourse it needs access via API).
The tool ensures all teammembers comply against standard rules for creating and tracking tasks per PBI.
The standard process can be configured as per your project's DONE criteria.

### Sample email notification sent to an individual team member
![Alt text](https://github.com/abhijeetd/TimeLog-Analysis-Tool/blob/master/SprintTimeManager/Documentation/TimeMachineEmailNotification.png?raw=true "Sample email notification sent to an individual team member")

Features
--------
1. Ability to define and enforce custom DONE criteria
2. Connect to any work tracking system that exposes data via API - In this version TFS support has been implemented, but it can be extended for other systems like Rally, Jira, etc.)
3. Ability to send analysis report to multiple consumers like 
* Email to individual teammember
* Dump in MongoDB
* Store data in RDBMS (for later analysis to discover patterns, etc. )

Configurations
--------------
The application is highly configurable. The application configurations are defined in a file named: TimeLogManagerManifest.xml

The configuration file consists of following sections:
* Organization - Title
* Project - Title
* IterationPath - Iteration path against which data needs to be analyzed
* Milestones - Define project milestones like "Code completion date", "Code freeze date", etc. Note all this configured data is available to analyzers - this helps developers use this information for custom logic/analysis.
* TeamProfiles - list of all team members
* Plugins - custom plugins to for 
* * retreiving data source
* * Pushing analysis data to multiple listeners
* * Analyzers that gets workitem and it's associated tasks with misc. details that can then by analyzed to implement DONE criteria
