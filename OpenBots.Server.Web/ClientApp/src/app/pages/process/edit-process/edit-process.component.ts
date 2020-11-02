import { Component, OnInit, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProcessService } from '../process.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NbToastrService } from '@nebular/theme';
import { UploadOutput, UploadInput, UploadFile, humanizeBytes, UploaderOptions } from 'ngx-uploader';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'ngx-edit-process',
  templateUrl: './edit-process.component.html',
  styleUrls: ['./edit-process.component.scss']
})
export class EditProcessComponent implements OnInit {
    //// file upload declartion ////
    options: UploaderOptions;
    files: UploadFile[];
    uploadInput: EventEmitter<UploadInput>;
    humanizeBytes: Function;
    dragOver: boolean;
  native_file: any;
  native_file_name: any;
    ///// end declartion////

value = ['Published', 'Commited'];
 showprocess: FormGroup;
 save_value: any = [];
 show_upload :boolean = false;
 submitted = false;
 process_id :any =[];
 show_allprocess: any = [];
 fileUploadId;

 constructor(private formBuilder: FormBuilder, private toastrService: NbToastrService, protected router: Router,
   protected processService: ProcessService,    private acroute: ActivatedRoute,) {
    this.acroute.queryParams.subscribe((params) => {
      this.process_id = params.id;
      this.get_process(params.id);
    });

 }

 get_process(id) {
  this.processService.getProcessId(id).subscribe((data:HttpResponse<any>) => {
    this.show_allprocess = data.body;
    localStorage.setItem('Etag',data.headers.get('ETag').replace(/\"/g, ''))
    this.showprocess.patchValue(data.body);

  });
}



 ngOnInit(): void {
   this.showprocess = this.formBuilder.group({
     name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100), Validators.pattern('^[A-Za-z0-9_.-]{3,100}$'),]],
     status: [''],
   });

 }

 onUploadOutput(output: UploadOutput): void {
  switch (output.type) {
     
    case 'addedToQueue':
      if (typeof output.file !== 'undefined') {

        // this.native_file = output.file.nativeFile
        // this.native_file_name = output.file.nativeFile.name
        this.native_file = output.file.nativeFile
        this.native_file_name = output.file.nativeFile.name
        // let formData = new FormData();
        // formData.append('file', this.native_file, this.native_file_name);
        // formData.append('name', this.showprocess.value.name);
        // //  formData.append('status', this.showprocess.value.status);
        // //     formData.append('Organizationid', this.showprocess.value.Organizationid);
        // //     formData.append('ApiComponent', this.showprocess.value.ApiComponent);
        // this.processService.uploadUpdateProcessFile(formData,this.process_id).subscribe((data:any) =>
        // {
        //   this.showprocess.value.binaryObjectId = data.binaryObjectId
        //   this.toastrService.success('File Upload Successfully','Success')
        
        // })
      }
      break;
     
  }
}


 

 get f() { return this.showprocess.controls; }

 
 onSubmit() {



   if (this.native_file) {
     let formData = new FormData();
     formData.append('file', this.native_file, this.native_file_name);
     formData.append('name', this.showprocess.value.name);
     // FileUploadformData.append('Organizationid', localStorage.getItem('ActiveOrganizationID'));
     this.processService.uploadUpdateProcessFile(formData, this.process_id).subscribe((data: any) => {
       this.showprocess.value.binaryObjectId = data.binaryObjectId
       this.toastrService.success('File Upload Successfully', 'Success')
       this.router.navigate(['pages/process/list']);
       this.native_file = undefined;
       this.native_file_name = undefined;
     })
   }
   else if (this.native_file == undefined) {
     let processobj = {
       'name': this.showprocess.value.name
     }
     this.processService.updateProcess(processobj, this.process_id).subscribe(
       (data) => {
         this.toastrService.success('Process Update  Successfully!', 'Success');
         this.router.navigate(['pages/process/list']);
         this.native_file = undefined;
         this.native_file_name = undefined;
       });
   }
       
 }

}
