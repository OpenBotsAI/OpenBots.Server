import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpService } from '../../../@core/services/http.service';
import { Agents } from '../../../interfaces/agnets';
import { NbDateService } from '@nebular/theme';
import { Router, ActivatedRoute } from '@angular/router';
import { CronOptions } from '../../../interfaces/cronJobConfiguration';

import { TimeDatePipe } from '../../../@core/pipe';
import { Processes } from '../../../interfaces/processes';

@Component({
  selector: 'ngx-add-schedule',
  templateUrl: './add-schedule.component.html',
  styleUrls: ['./add-schedule.component.scss'],
})
export class AddScheduleComponent implements OnInit {
  scheduleForm: FormGroup;
  allAgents: Agents[] = [];
  allProcesses: Processes[] = [];
  isSubmitted = false;
  min: Date;
  max: Date;
  myDate: TimeDatePipe;
  currentScheduleId: string;
  title = 'Add';
  status = [
    { isDisabled: false, name: 'Enable' },
    { isDisabled: true, name: 'Disable' },
  ];
  radioaButton = ['oneTime', 'recurrence'];

  cronExpression = '0/0 * 0/0 * *';
  isCronDisabled = false;
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
    private fb: FormBuilder,
    private httpService: HttpService,
    private dateService: NbDateService<Date>,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.currentScheduleId = this.route.snapshot.params['id'];
    this.scheduleForm = this.initScheduleForm();
    this.getAllAgents();
    this.getProcessesLookup();
    if (this.currentScheduleId) {
      this.title = 'Update';
      this.getScheduleById();
    }
    this.min = new Date();
    this.max = new Date();
    this.min = this.dateService.addMonth(this.dateService.today(), 0);
    this.max = this.dateService.addMonth(this.dateService.today(), 1);
  }

  initScheduleForm() {
    return this.fb.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
        ],
      ],
      agentId: ['', [Validators.required]],
      processId: ['', [Validators.required]],
      isDisabled: [false],
      cronExpression: [''],
      projectId: [''],
      recurrence: [],
      startingType: ['', [Validators.required]],
      status: [''],
      expiryDate: [''],
      startDate: ['', [Validators.required]],
    });
  }

  get formControls() {
    return this.scheduleForm.controls;
  }

  getAllAgents(): void {
    this.httpService.get(`Agents/GetLookup`).subscribe((response) => {
      if (response && response.length !== 0) this.allAgents = [...response];
      else this.allAgents = [];
    });
  }

  onScheduleSubmit(): void {
    this.isSubmitted = true;
    if (this.scheduleForm.value.startDate) {
      this.myDate = new TimeDatePipe();
      this.scheduleForm.value.startDate = this.myDate.transform(
        this.scheduleForm.value.startDate,
        'lll'
      );
    }
    if (this.scheduleForm.value.expiryDate) {
      this.myDate = new TimeDatePipe();
      this.scheduleForm.value.expiryDate = this.myDate.transform(
        this.scheduleForm.value.expiryDate,
        'lll'
      );
    }
    if (this.cronExpression !== '0/0 * 0/0 * *') {
      this.scheduleForm.value.cronExpression = this.cronExpression;
    }

    if (this.currentScheduleId) this.updateSchedule();
    else this.addSchedule();
  }

  updateSchedule(): void {
    this.httpService
      .put(`Schedules/${this.currentScheduleId}`, this.scheduleForm.value)
      .subscribe(
        () => {
          this.isSubmitted = false;
          this.httpService.success('Schedule updated successfully');
          this.router.navigate(['/pages/schedules']);
        },
        () => (this.isSubmitted = false)
      );
  }

  addSchedule(): void {
    this.httpService
      .post('Schedules', this.scheduleForm.value, { observe: 'response' })
      .subscribe(
        (response) => {
          if (response && response.status === 201) {
            this.httpService.success('Schedule has created successfully');
            this.scheduleForm.reset();
            this.isSubmitted = false;
          }
          this.router.navigate(['/pages/schedules']);
        },
        () => (this.isSubmitted = false)
      );
  }

  getScheduleById(): void {
    this.httpService
      .get(`Schedules/${this.currentScheduleId}`)
      .subscribe((response) => {
        if (response) {
          this.cronExpression = response.cronExpression;
          this.scheduleForm.patchValue(response);
        }
      });
  }

  getProcessesLookup(): void {
    this.httpService.get(`Processes/GetLookup`).subscribe((response) => {
      if (response) this.allProcesses = [...response];
    });
  }

  radioSetValidator(value: string): void {
    if (value === 'recurrence') {
      this.scheduleForm.get('expiryDate').setValidators([Validators.required]);
      this.scheduleForm.get('expiryDate').updateValueAndValidity();
    } else if (value === 'manual') {
      this.scheduleForm.get('startDate').clearValidators();
      this.scheduleForm.get('startDate').updateValueAndValidity();
    }
  }
}
