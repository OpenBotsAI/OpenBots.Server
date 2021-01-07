import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { AgentsService } from '../agents.service';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-edit-agents',
  templateUrl: './edit-agents.component.html',
  styleUrls: ['./edit-agents.component.scss'],
})
export class EditAgentsComponent implements OnInit {
  addagent: FormGroup;
  submitted = false;
  agent_id: any = [];
  cred_value: any = [];
  show_allagents: any = [];
  etag;
  checked = false;
  constructor(
    private acroute: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    protected agentService: AgentsService,
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
      ipAddresses: [
        '',
        [
          Validators.pattern(
            '^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?).){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|(::[1])$'
          ),
        ],
      ],
      isEnabled: [''],
      CredentialId: ['', [Validators.required]],
    });
  }

  get_allagent(id) {
    this.agentService.getAgentbyID(id).subscribe((data: HttpResponse<any>) => {
      console.log(data)
      this.show_allagents = data.body;
      if (this.show_allagents.macAddresses != null) {
        this.checked = true;
      } else if (this.show_allagents.macAddresses == null || this.show_allagents.macAddresses == '') {
        this.checked = false
      }
      console.log(data.headers.get('ETag').replace(/\"/g, ''))
      this.etag = data.headers.get('ETag').replace(/\"/g, '')
      this.addagent.patchValue(this.show_allagents);

      this.addagent.patchValue({
        CredentialId: this.show_allagents.credentialId,
      }
      )
    });
  }
  get_cred() {
    this.agentService.getCred().subscribe((data: any) => {
      this.cred_value = data;
    });
  }
  check(checked: boolean) {

    this.checked = checked;
    console.log(this.checked)
  }

  get f() {
    return this.addagent.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.agentService.editAgent(this.agent_id, this.addagent.value, this.etag).subscribe(
      (data) => {
        this.toastrService.success('Updated successfully', 'Success');
        this.router.navigate(['pages/agents/list']);
      },
      (error) => {
        // console.log(error.error.status)
        if (error.error.status === 409) {
          this.toastrService.danger(error.error.serviceErrors, 'error')
          this.get_allagent(this.agent_id)
          this.submitted = false
        }
        if (error.error.status === 429) {
          this.toastrService.danger(error.error.serviceErrors, 'error')
          // this.get_allagent(this.agent_id)
          this.submitted = false
        }
      }
    );
  }

  onReset() {
    this.submitted = false;
    this.addagent.reset();
  }

  handleInput(event) {
    var key = event.keyCode;
    if (key === 32) {
      event.preventDefault();
      return false;
    }
  }
}
