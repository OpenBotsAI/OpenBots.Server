import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GetBinaryIdComponent } from './get-binary-id.component';

describe('GetBinaryIdComponent', () => {
  let component: GetBinaryIdComponent;
  let fixture: ComponentFixture<GetBinaryIdComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GetBinaryIdComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GetBinaryIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
