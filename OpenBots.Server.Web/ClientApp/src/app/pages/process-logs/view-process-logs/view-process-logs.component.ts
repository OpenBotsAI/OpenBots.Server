import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { JsonEditorComponent, JsonEditorOptions } from 'ang-jsoneditor';
import { HttpService } from '../../../@core/services/http.service';
import { Agents } from '../../../interfaces/agnets';
import { TimeDatePipe } from '../../../@core/pipe';
import { NgxXml2jsonService } from 'ngx-xml2json';

@Component({
  selector: 'ngx-view-process-logs',
  templateUrl: './view-process-logs.component.html',
  styleUrls: ['./view-process-logs.component.scss'],
})
export class ViewProcessLogsComponent implements OnInit, AfterViewInit {
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
    private router: Router,
    private ngxXml2jsonService: NgxXml2jsonService
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

  ngAfterViewInit() {

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
      processId: [''],
      agentId: [''],
      machineName: [''],
      processName: [''],
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
      .get(`processlogs/${this.processLogId}`)
      .subscribe((response) => {
        if (response) {
          this.agentId = response.agentId;
          this.pocessId = response.processId;
          response.processLogTimeStamp = this.transformDateTime(
            response.processLogTimeStam,
            'lll'
          );
          response.createdOn = this.transformDateTime(
            response.createdOn,
            'lll'
          );

          const parser = new DOMParser();
          const xml = parser.parseFromString(response.properties, 'text/xml');
          const obj = this.ngxXml2jsonService.xmlToJson(xml);
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
        PageName: 'OpenBots.Server.Model.ExecutionLog',
        id: this.processLogId,
      },
    });
  }

  navigateToProcess(): void {
    this.router.navigate(['/pages/process/get-process-id'], {
      queryParams: { id: this.pocessId },
    });
  }
}
