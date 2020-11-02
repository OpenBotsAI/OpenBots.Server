import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewProcessLogsComponent } from './view-process-logs.component';

describe('ViewProcessLogsComponent', () => {
  let component: ViewProcessLogsComponent;
  let fixture: ComponentFixture<ViewProcessLogsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewProcessLogsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewProcessLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
