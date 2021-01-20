import { Component, OnInit, TemplateRef } from '@angular/core';
import { FormGroup, FormBuilder, FormArray, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { ActivatedRoute, Router } from '@angular/router';
import { TimeDatePipe } from '../../../@core/pipe';
import { CronOptions } from '../../../interfaces/cronJobConfiguration';
import { Agents } from '../../../interfaces/agnets';
import { Schedule } from '../../../interfaces/schedule';
import { Automation } from '../../../interfaces/automations';
import {
  AgentApiUrl,
  automationsApiUrl,
  SchedulesApiUrl,
} from '../../../webApiUrls';
import { HelperService } from '../../../@core/services/helper.service';
import { DialogService } from '../../../@core/dialogservices';

@Component({
  selector: 'ngx-view-schedule',
  templateUrl: './view-schedule.component.html',
  styleUrls: ['./view-schedule.component.scss'],
})
export class ViewScheduleComponent implements OnInit {
  scheduleForm: FormGroup;
  currentScheduleId: string;
  pipe: TimeDatePipe;
  allAgents: Agents[] = [];
  allProcesses: Automation[] = [];
  cronExpression = '0/0 * 0/0 * *';
  scheduleData: Schedule;
  jobRunNowForm: FormGroup;
  dataType = ['Text', 'Number'];
  items: FormArray;
  parameters: any[] = [];
  isDisabled = false;
  cronOptions: CronOptions = {
    formInputClass: 'form-control cron-editor-input',
    formSelectClass: 'form-control cron-editor-select',
    formRadioClass: 'cron-editor-radio',
    formCheckboxClass: 'cron-editor-checkbox',
    defaultTime: '10:00:00',
    use24HourTime: true,
    hideMinutesTab: false,
    hideHourlyTab: false,
    hideDailyTab: false,
    hideWeeklyTab: false,
    hideMonthlyTab: false,
    hideYearlyTab: true,
    hideAdvancedTab: true,
    hideSeconds: true,
    removeSeconds: true,
    removeYears: true,
  };
  constructor(
    protected router: Router,
    private fb: FormBuilder,
    private httpService: HttpService,
    private route: ActivatedRoute,
    private helperService: HelperService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.currentScheduleId = this.route.snapshot.params['id'];
    this.getAllAgents();
    this.getAllProcesses();
    this.scheduleForm = this.initScheduleForm();
    this.jobRunNowForm = this.initializeJobRunNowForm();
    if (this.currentScheduleId) {
      this.getScheduleById();
    }
    this.jobRunNowForm = new FormGroup({
      items: new FormArray([]),
    });
  }

  initializeJobRunNowForm(): FormGroup {
    return this.fb.group({
      Name: ['', [Validators.required]],
      DataType: ['Text', [Validators.required]],
      Value: ['', [Validators.required]],
    });
  }
  initScheduleForm() {
    return this.fb.group({
      id: [''],
      agentId: [''],
      automationId: [''],
      agentName: [''],
      createdBy: [''],
      createdOn: [''],
      cronExpression: [''],
      expiryDate: [''],
      name: [''],
      projectId: [''],
      recurrence: [],
      startDate: [''],
      startingType: [''],
      status: [''],
      updatedBy: [''],
      updatedOn: [],
      isDisabled: [''],
    });
  }

  getScheduleById() {
    this.httpService
      .get(
        `${SchedulesApiUrl.schedules}/${SchedulesApiUrl.view}/${this.currentScheduleId}`
      )
      .subscribe((response) => {
        if (response) {
          response.startDate = this.helperService.transformDate(
            response.startDate,
            'lll'
          );
          response.expiryDate = this.helperService.transformDate(
            response.expiryDate,
            'lll'
          );
          response.createdOn = this.helperService.transformDate(
            response.createdOn,
            'lll'
          );
          response.updatedOn = this.helperService.transformDate(
            response.updatedOn,
            'lll'
          );
          this.cronExpression = response.cronExpression;
          this.scheduleData = { ...response };
          this.parameters = response.scheduleParameters;

          this.scheduleForm.patchValue(response);
          this.scheduleForm.disable();
        }
      });
  }

  setvalues(parameters): FormArray {
    const formArray = new FormArray([]);
    parameters.forEach((param) => {
      formArray.push(
        this.fb.group({
          Name: param.name,
          DataType: param.dataType,
          Value: param.value,
        })
      );
    });
    return formArray;
  }

  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], {
      queryParams: {
        PageName: 'Schedule',
        id: this.currentScheduleId,
      },
    });
  }

  getAllAgents(): void {
    this.httpService
      .get(`${AgentApiUrl.Agents}/${AgentApiUrl.getLookup}`)
      .subscribe((response) => {
        if (response && response.length) this.allAgents = [...response];
        else this.allAgents = [];
      });
  }

  getAllProcesses(): void {
    this.httpService
      .get(`${automationsApiUrl.getLookUp}`)
      .subscribe((response) => {
        if (response && response.length !== 0)
          this.allProcesses = [...response];
        else this.allProcesses = [];
      });
  }

  runNowJob(ref: TemplateRef<any>): void {
    this.dialogService.openDialog(ref);

    // this.jobRunNowForm.setControl(
    //   'parameters',
    //   this.setvalues(this.parameters)
    // );
    this.jobRunNowForm.setControl('items', this.setvalues(this.parameters));
    this.jobRunNowForm.patchValue(this.parameters);
    console.log('value', this.jobRunNowForm.value);
    // `${SchedulesApiUrl.schedules}/${automationsApiUrl.automation}/${this.scheduleData.automationId}/${SchedulesApiUrl.runNow}?AgentId=${this.scheduleData.agentId}`
    // const obj = {
    //   agentId: this.scheduleData.agentId,
    //   automationId: this.scheduleData.automationId,
    // };
    // this.httpService
    //   .post(`${SchedulesApiUrl.schedules}/${SchedulesApiUrl.runNow}`, obj, {
    //     observe: 'response',
    //   })
    //   .subscribe((response) => {
    //     console.log('res', response);
    //     if (response && response.status === 200)
    //       this.httpService.success('Job created successfully');
    //   });
  }

  runJobNow(ref): void {
    this.isDisabled = true;
    let obj;
    if (this.jobRunNowForm.value.items.length) {
      obj = {
        agentId: this.scheduleData.agentId,
        automationId: this.scheduleData.automationId,
        JobParameters: [...this.jobRunNowForm.value.items],
      };
    } else {
      obj = {
        agentId: this.scheduleData.agentId,
        automationId: this.scheduleData.automationId,
      };
    }
    this.httpService
      .post(`${SchedulesApiUrl.schedules}/${SchedulesApiUrl.runNow}`, obj, {
        observe: 'response',
      })
      .subscribe(
        (response) => {
          if (response && response.status === 200) {
            this.isDisabled = false;
            ref.close();
            if (this.items && this.items.length) this.items.clear();
            this.httpService.success('Job created successfully');
          }
        },
        () => (this.isDisabled = false)
      );
  }

  // below code is working just commented just for now
  // addJobParameter(): void {
  //   this.items = this.jobRunNowForm.get('items') as FormArray;
  //   this.items.push(this.initializeJobRunNowForm());
  // }

  get formControls() {
    return this.jobRunNowForm.get('items') as FormArray;
  }

  clear(ref): void {
    ref.close();
    if (this.items && this.items.length) this.items.clear();
  }
}
