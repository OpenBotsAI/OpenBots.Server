import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { IpVersion, RxwebValidators } from '@rxweb/reactive-form-validators';
import { HttpService } from '../../../@core/services/http.service';
import { AgentApiUrl, CredentialsApiUrl } from '../../../webApiUrls';
import { HelperService } from '../../../@core/services/helper.service';

@Component({
  selector: 'ngx-add-agents',
  templateUrl: './add-agents.component.html',
  styleUrls: ['./add-agents.component.scss'],
})
export class AddAgentsComponent implements OnInit {
  agentForm: FormGroup;
  etag;
  title = 'Add';
  checked = false;
  submitted = false;
  credentialArr: { credentialId: string; credentialName: string }[] = [];
  value = ['JSON', 'Number', 'Text'];
  ipVersion = 'V4';
  urlId: string;
  show_allagents: any = [];
  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private httpService: HttpService,
    private helperService: HelperService
  ) {}

  ngOnInit(): void {
    this.urlId = this.route.snapshot.params['id'];
    this.initializeAgentForm();

    if (this.urlId) {
      this.getAgentById();
      this.title = 'Update';
    }
    this.getCredentials();
  }

  initializeAgentForm(): void {
    this.agentForm = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(100),
          Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),
        ],
      ],
      machineName: [''],
      macAddresses: [''],
      ipAddresses: [''],
      isEnabled: [true],
      CredentialId: ['', [Validators.required]],
      userName: this.urlId ? [''] : ['', [Validators.required]],
      password: this.urlId ? [''] : ['', [Validators.required]],
      ipOption: [''],
      isEnhancedSecurity: false,
    });
  }

  getCredentials(): void {
    this.httpService
      .get(`${CredentialsApiUrl.credentials}/${CredentialsApiUrl.getLookUp}`)
      .subscribe((data) => {
        if (data) this.credentialArr = [...data];
        else this.credentialArr = [];
      });
  }

  get formControl() {
    return this.agentForm.controls;
  }
  check(checked: boolean) {
    this.checked = checked;
    if (checked) {
      this.agentForm.get('ipOption').setValidators([Validators.required]);
      this.agentForm.controls['ipOption'].setValue('ipv4');
      this.agentForm
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.agentForm
        .get('macAddresses')
        .setValidators([
          Validators.required,
          Validators.pattern('^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$'),
        ]);
      this.agentForm.get('ipOption').updateValueAndValidity();
      this.agentForm.get('macAddresses').updateValueAndValidity();
      this.agentForm.get('ipAddresses').updateValueAndValidity();
    } else {
      this.agentForm.get('ipOption').reset();
      this.agentForm.get('ipAddresses').reset();
      this.agentForm.get('macAddresses').reset();
      this.agentForm.get('ipOption').clearValidators();
      this.agentForm.get('ipOption').updateValueAndValidity();
      this.agentForm.get('ipAddresses').clearValidators();
      this.agentForm.get('ipAddresses').updateValueAndValidity();
      this.agentForm.get('macAddresses').clearValidators();
      this.agentForm.get('macAddresses').updateValueAndValidity();
    }
  }

  onSubmit() {
    if (this.urlId) {
      this.updateAgent();
    } else {
      this.addAgent();
    }
  }

  addAgent(): void {
    this.submitted = true;
    if (this.agentForm.invalid) {
      return;
    }
    this.httpService
      .post(`${AgentApiUrl.Agents}`, this.agentForm.value)
      .subscribe(
        () => {
          this.httpService.success('Agent added successfully');
          this.router.navigate(['pages/agents/list']);
        },
        () => (this.submitted = false)
      );
  }

  updateAgent(): void {
    this.submitted = true;
    const headers = this.helperService.getETagHeaders(this.etag);
    this.httpService
      .put(`${AgentApiUrl.Agents}/${this.urlId}`, this.agentForm.value, headers)
      .subscribe(
        () => {
          this.httpService.success('Updated successfully');
          this.router.navigate(['pages/agents/list']);
        },
        (error) => {
          if (error.error.status === 409) {
            this.httpService.error(error.error.serviceErrors);
            this.getAgentById();
            this.submitted = false;
          }
          if (error.error.status === 429) {
            this.httpService.error(error.error.serviceErrors);
            this.submitted = false;
          }
        }
      );
  }

  onReset() {
    this.submitted = false;
    this.agentForm.reset();
  }

  handleInput(event) {
    var key = event.keyCode;
    if (key === 32) {
      event.preventDefault();
      return false;
    }
  }

  radioSetValidator(value: string): void {
    this.agentForm.get('ipAddresses').clearValidators();
    this.agentForm.get('ipAddresses').reset();
    if (value === 'ipv4') {
      this.ipVersion = 'V4';
      this.agentForm
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V4 }),
        ]);
      this.agentForm.get('ipAddresses').updateValueAndValidity();
    } else {
      this.ipVersion = 'V6';
      this.agentForm
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6 }),
        ]);
      this.agentForm.get('ipAddresses').updateValueAndValidity();
    }
  }

  getAgentById(): void {
    this.httpService
      .get(`${AgentApiUrl.Agents}/${this.urlId}`, { observe: 'response' })
      .subscribe((data) => {
        console.log('data', data);
        if (data && data.body) {
          this.show_allagents = data.body;
          if (data.body.ipOption === 'ipv6') {
            this.agentForm
              .get('ipAddresses')
              .setValidators([
                Validators.required,
                RxwebValidators.ip({ version: IpVersion.V6 }),
              ]);
            this.agentForm.get('ipAddresses').updateValueAndValidity();
            this.ipVersion = 'V6';
          } else if (data.body.ipOption === 'ipv4') {
            this.agentForm
              .get('ipAddresses')
              .setValidators([
                Validators.required,
                RxwebValidators.ip({ version: IpVersion.V4 }),
              ]);
            this.agentForm.get('ipAddresses').updateValueAndValidity();
          }
          this.etag = data.headers.get('ETag').replace(/\"/g, '');
          this.agentForm.patchValue(this.show_allagents);
          this.agentForm.patchValue({
            CredentialId: this.show_allagents.credentialId,
          });
        }
      });
  }
}
