import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { Router } from '@angular/router';
import { IpVersion, RxwebValidators } from '@rxweb/reactive-form-validators';

@Component({
  selector: 'ngx-add-agents',
  templateUrl: './add-agents.component.html',
  styleUrls: ['./add-agents.component.scss'],
})
export class AddAgentsComponent implements OnInit {
  addagent: FormGroup;
  checked = false;
  submitted = false;
  cred_value: any = [];
  value = ['JSON', 'Number', 'Text'];
  ipVersion = 'V4';
  constructor(
    private formBuilder: FormBuilder,
    private agentService: AgentsService,
    private router: Router,
    private toastrService: NbToastrService
  ) {}

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
      machineName: [''],
      macAddresses: [''],
      ipAddresses: ['', [RxwebValidators.ip({ version: IpVersion.V4 })]],
      isEnabled: [true],
      CredentialId: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      password: ['', [Validators.required]],
      ipOption: ['ipv4'],
      isEnhancedSecurity: false,
    });

    this.get_cred();
  }
  get_cred() {
    this.agentService.getCred().subscribe((data: any) => {
      this.cred_value = data;
    });
  }
  get f() {
    return this.addagent.controls;
  }
  check(checked: boolean) {
    this.checked = checked;
    if (checked) {
      this.addagent
        .get('macAddresses')
        .setValidators([Validators.required, RxwebValidators.mac()]);
      this.addagent.get('ipAddresses').setValidators([Validators.required]);
      this.addagent.get('macAddresses').updateValueAndValidity();
      this.addagent.get('ipAddresses').updateValueAndValidity();
    } else {
      this.addagent.get('ipAddresses').clearValidators();
      this.addagent.get('ipAddresses').updateValueAndValidity();
      this.addagent.get('macAddresses').clearValidators();
      this.addagent.get('macAddresses').updateValueAndValidity();
    }
  }

  onSubmit() {
    this.submitted = true;
    if (this.addagent.invalid) {
      return;
    }
    this.agentService.addAgent(this.addagent.value).subscribe(
      () => {
        this.toastrService.success('Agent added successfully', 'Success');
        this.router.navigate(['pages/agents/list']);
      },
      () => {
        this.submitted = false;
      }
    );
  }

  onReset() {
    this.submitted = false;
    this.addagent.reset();
  }

  keyPressAlphaNumericWithCharacters(event) {
    var inp = String.fromCharCode(event.keyCode);
    if (/[a-zA-Z0-9-/. ]/.test(inp)) {
      return true;
    } else {
      event.preventDefault();
      return false;
    }
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
      this.addagent.updateValueAndValidity();
    } else {
      this.ipVersion = 'V6';
      this.addagent
        .get('ipAddresses')
        .setValidators([
          Validators.required,
          RxwebValidators.ip({ version: IpVersion.V6 }),
        ]);
      this.addagent.updateValueAndValidity();
    }
  }
}
