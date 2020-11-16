import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AgentsService } from '../agents.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TimeDatePipe } from '../../../@core/pipe';

@Component({
  selector: 'ngx-get-agents-id',
  templateUrl: './get-agents-id.component.html',
  styleUrls: ['./get-agents-id.component.scss']
})
export class GetAgentsIdComponent implements OnInit {
  show_allagents: any = [];
  cred_value: any =[] ;
  addagent: FormGroup;
  constructor(private acroute: ActivatedRoute, protected router: Router,
    protected agentService: AgentsService, private formBuilder: FormBuilder,) {
    this.acroute.queryParams.subscribe(params => {
      this.get_allagent(params.id);
    });
  }

  ngOnInit(): void {
    this.addagent = this.formBuilder.group({
      name: [''],
      machineName: [''],
      macAddresses: [''],
      ipAddresses: [''],
      credentialId: [''],
      credentialName: [''],
      isEnabled: [''],
      createdBy: [''],
      createdOn: [''],
      deleteOn: [''],
      deletedBy: [''],
      id: [''],
      isDeleted: [''],
      isHealthy: [''],
      lastReportedMessage: [''],
      lastReportedOn: [''],
      lastReportedStatus: [''],
      lastReportedWork: [''],
      timestamp: [''],
      updatedBy: [''],
      updatedOn: [''],
    });
   
  }

 

  get_allagent(id) {
    this.agentService.getAgentbyID(id).subscribe(
      (data: any) => {
        this.show_allagents = data.body;        
        const filterPipe = new TimeDatePipe();
        this.show_allagents.lastReportedOn = filterPipe.transform(this.show_allagents.lastReportedOn, 'lll');
        if (this.show_allagents.isHealthy == true) {
          this.show_allagents.isHealthy = "yes";
        }
        else if (this.show_allagents.isHealthy == false) {
          this.show_allagents.isHealthy = "No";
        }
        this.addagent.patchValue(this.show_allagents);
        this.addagent.disable();
    
      });
      
  }


  gotoaudit() {
    this.router.navigate(['/pages/change-log/list'], { queryParams: { PageName: 'OpenBots.Server.Model.AgentModel', id: this.show_allagents.id } })
  }

}
