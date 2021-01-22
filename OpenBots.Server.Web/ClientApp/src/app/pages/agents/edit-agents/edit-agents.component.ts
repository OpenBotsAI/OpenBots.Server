import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { HttpResponse } from '@angular/common/http';
import { IpVersion, RxwebValidators } from '@rxweb/reactive-form-validators';

@Component({
  selector: 'ngx-edit-agents',
  templateUrl: './edit-agents.component.html',
  styleUrls: ['./edit-agents.component.scss'],
})
export class EditAgentsComponent implements OnInit {
  addagent: FormGroup;
  isSubmitted = false;
  agent_id: any = [];
  cred_value: any = [];
  show_allagents: any = [];
  etag;
  checked = false;
  ipVersion = 'V4';
  constructor(
    private acroute: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    private agentService: AgentsService,
    private toastrService: NbToastrService
  ) {
    this.acroute.queryParams.subscribe((params) => {
      this.agent_id = params.id;
      this.get_allagent(params.id);
    });
    this.get_cred();
  }

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      machineName: ['', [Validators.required]],
      macAddresses: [''],
      ipAddresses: [''],
      isEnabled: [''],
      CredentialId: ['', [Validators.required]],
      ipOption: [''],
      isEnhancedSecurity: false,
    });
  }

  get_allagent(id) {
    this.agentService.getAgentbyID(id).subscribe((data: HttpResponse<any>) => {
      if (data && data.body) {
        this.show_allagents = data.body;
        if (data.body.ipOption === 'ipv6') {
          this.ipVersion = 'V6';
        }
        this.etag = data.headers.get('ETag').replace(/\"/g, '');
        this.addagent.patchValue(this.show_allagents);
        this.addagent.patchValue({
          CredentialId: this.show_allagents.credentialId,
        });
      }
    });
  }
  get_cred() {
    this.agentService.getCred().subscribe((data: any) => {
      this.cred_value = data;
    });
  }

  get f() {
    return this.addagent.controls;
  }

  onSubmit() {
    this.isSubmitted = true;
    this.agentService
      .editAgent(this.agent_id, this.addagent.value, this.etag)
      .subscribe(
        () => {
          this.toastrService.success('Updated successfully', 'Success');
          this.router.navigate(['pages/agents/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            this.get_allagent(this.agent_id);
            this.isSubmitted = false;
          }
          if (error.error.status === 429) {
            this.toastrService.danger(error.error.serviceErrors, 'error');
            // this.get_allagent(this.agent_id)
            this.isSubmitted = false;
          }
        }
      );
  }

  handleInput(event) {
    var key = event.keyCode;
    if (key === 32) {
      event.preventDefault();
      return false;
    }
  }

  radioSetValidator(value: string): void {
    this.addagent.get('ipAddresses').clearValidators();
    this.addagent.get('ipAddresses').reset();
    if (value === 'ipv4') {
      this.ipVersion = 'V4';
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.addagent.get('ipAddresses').updateValueAndValidity();
    } else {
      this.ipVersion = 'V6';
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6 }),
        ]);
      this.addagent.get('ipAddresses').updateValueAndValidity();
    }
  }

  check(checked: boolean) {
    this.checked = checked;
    if (checked) {
      this.addagent
        .get('macAddresses')
        .setValidators([Validators.required, RxwebValidators.mac()]);
      this.addagent.get('macAddresses').updateValueAndValidity();
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.addagent.get('ipAddresses').updateValueAndValidity();
    } else {
      this.addagent.get('ipAddresses').clearValidators();
      this.addagent.get('ipAddresses').updateValueAndValidity();
      this.addagent.get('macAddresses').clearValidators();
      this.addagent.get('macAddresses').updateValueAndValidity();
    }
  }
}
