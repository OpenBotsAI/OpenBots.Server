import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { HttpService } from '../../../@core/services/http.service';
import { Agents } from '../../../interfaces/agnets';
import { TimeDatePipe } from '../../../@core/pipe';

@Component({
  selector: 'ngx-view-automation-logs',
  templateUrl: './view-automation-logs.component.html',
  styleUrls: ['./view-automation-logs.component.scss'],
})
export class ViewAutomationLogsComponent implements OnInit {
  processLogId: string;
  processLogsForm: FormGroup;
  pipe: TimeDatePipe;
  agentLookUp: Agents[] = [];
  agentId: string;
  pocessId: string;
  public editorOptions: JsonEditorOptions;
  public data: any;
  aceXmlValue: any;
  @ViewChild(JsonEditorComponent) editor: JsonEditorComponent;

  constructor(
    private httpService: HttpService,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.processLogId = this.route.snapshot.params['id'];
    this.processLogsForm = this.initializeForm();
    this.editorOptions = new JsonEditorOptions();

    if (this.processLogId) {
      this.getAgentLookup();
      this.getProcessById();
    }
  }

  initializeForm() {
    return this.fb.group({
      agentName: [''],
      message: [''],
      messageTemplate: [''],
      level: [''],
      processLogTimeStamp: [],
      exception: [''],
      properties: [''],
      jobId: [''],
      automationId: [''],
      agentId: [''],
      machineName: [''],
      automationName: [''],
      logger: [''],
      id: [''],
      isDeleted: [],
      createdBy: [''],
      createdOn: [],
      deletedBy: [''],
      deleteOn: [],
      timestamp: [''],
      updatedOn: [],
      updatedBy: [''],
    });
  }

  getProcessById() {
    this.httpService
      .get(`AutomationLogs/${this.processLogId}`)
      .subscribe((response) => {
        if (response) {
          this.agentId = response.agentId;
          this.pocessId = response.automationId;
          response.processLogTimeStamp = this.transformDateTime(
            response.processLogTimeStam,
            'lll'
          );
          response.createdOn = this.transformDateTime(
            response.createdOn,
            'lll'
          );
          this.processLogsForm.patchValue(response);
          this.processLogsForm.disable();
        }
      });
  }

  transformDateTime(value: string, format: string) {
    this.pipe = new TimeDatePipe();
    return this.pipe.transform(value, `${format}`);
  }

  getAgentLookup(): void {
    this.httpService.get(`Agents/GetLookup`).subscribe((response) => {
      if (response) {
        this.agentLookUp = [...response];
      }
    });
  }

  navigateToAgent(): void {
    this.router.navigate(['/pages/agents/get-agents-id'], {
      queryParams: { id: this.agentId },
    });
  }

  navigateToAudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        // PageName: 'ExecutionLog',
        pageName: 'AutomationLog',
        id: this.processLogId,
      },
    });
  }

  navigateToProcess(): void {
    this.router.navigate(['/pages/automation/get-automation-id'], {
      queryParams: { id: this.pocessId },
    });
  }
}
