import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllQueuesComponent } from './all-queues.component';

describe('AllQueuesComponent', () => {
  let component: AllQueuesComponent;
  let fixture: ComponentFixture<AllQueuesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllQueuesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllQueuesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
