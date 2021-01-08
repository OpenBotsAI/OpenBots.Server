import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
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
    private helperService: HelperService
  ) { }

  ngOnInit(): void {
    this.currentScheduleId = this.route.snapshot.params['id'];
    this.getAllAgents();
    this.getAllProcesses();
    this.scheduleForm = this.initScheduleForm();
    if (this.currentScheduleId) {
      this.getScheduleById();
    }
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
      .get(`${SchedulesApiUrl.schedules}/${this.currentScheduleId}`)
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
          this.scheduleForm.patchValue(response);
          this.scheduleForm.disable();
        }
      });
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

  runNowJob(): void {
    this.httpService
      .post(
        `${SchedulesApiUrl.schedules}/${automationsApiUrl.automation}/${this.scheduleData.automationId}/${SchedulesApiUrl.runNow}?AgentId=${this.scheduleData.agentId}`
      )
      .subscribe(() => this.httpService.success('Job created successfully'));
  }
}
