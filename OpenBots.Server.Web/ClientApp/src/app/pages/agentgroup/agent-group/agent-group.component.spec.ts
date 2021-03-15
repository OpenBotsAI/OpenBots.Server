import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AgentGroupComponent } from './agent-group.component';

describe('AgentGroupComponent', () => {
  let component: AgentGroupComponent;
  let fixture: ComponentFixture<AgentGroupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AgentGroupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AgentGroupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
