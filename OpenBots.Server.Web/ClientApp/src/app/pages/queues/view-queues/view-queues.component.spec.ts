import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewQueuesComponent } from './view-queues.component';

describe('ViewQueuesComponent', () => {
  let component: ViewQueuesComponent;
  let fixture: ComponentFixture<ViewQueuesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ViewQueuesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewQueuesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
