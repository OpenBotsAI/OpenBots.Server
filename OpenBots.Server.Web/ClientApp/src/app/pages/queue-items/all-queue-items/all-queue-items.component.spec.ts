import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllQueueItemsComponent } from './all-queue-items.component';

describe('AllQueueItemsComponent', () => {
  let component: AllQueueItemsComponent;
  let fixture: ComponentFixture<AllQueueItemsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllQueueItemsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllQueueItemsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
