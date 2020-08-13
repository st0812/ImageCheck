#!python3

from PIL import Image
import numpy as np
import colorsys
import matplotlib.pyplot as plt
from sklearn.cluster import KMeans




class HSVColorRegion:
    def setHueRegion(self,huestart,hueend):
        self.huestart=huestart
        self.hueend=hueend
    def isHueInSelectedRegion(self,h):
        if self.huestart<self.hueend:
            return self.huestart < h and h<self.hueend
        else :
            return h<self.hueend or self.huestart<h

    def setSaturationRegion(self,saturationstart,saturationend):
        self.saturationstart=saturationstart
        self.saturationend=saturationend
    def isSaturationInSelectedRegion(self,s):
        return self.saturationstart < s and s<self.saturationend
      
    def setValueRegion(self,valuestart,valueend):
        self.valuestart=valuestart
        self.valueend=valueend
    def isValueInSelectedRegion(self,v):
        return self.valuestart < v and v<self.valueend 

    def isColorInSelectedRegion(self,h,s,v):
        return self.isHueInSelectedRegion(h) and self.isSaturationInSelectedRegion(s) and self.isValueInSelectedRegion(v)

def hsv_to_normalizedhsv(h,s,v):
    return h/360,s/100,v/100

def normalizedhsv_to_hsv(h,s,v):
    return h*360,s*100,v*100

def getHSVImageAndClusteringModel(srcrgb,colornum,colortarget):
    hsv=np.apply_along_axis(lambda a:colorsys.rgb_to_hsv(*a),2,srcrgb/255)
    hsv[:,:,0],hsv[:,:,1],hsv[:,:,2]=normalizedhsv_to_hsv(hsv[:,:,0],hsv[:,:,1],hsv[:,:,2])
    fHsv=hsv.reshape(-1,3)
    a=list(filter(lambda x:colortarget.isColorInSelectedRegion(x[0],x[1],x[2]),fHsv))
    b=np.array(a).astype(np.int32)
    kmeans_model = KMeans(n_clusters=4, random_state=10).fit(b)
    return hsv,kmeans_model


def replaceTargetColorRegion(srchsv,clustering_model,colortarget):
    dsthsv=np.zeros(srchsv.shape)
    for i in range(0,srchsv.shape[0]):
        for j in range(0,srchsv.shape[1]):
            if colortarget.isColorInSelectedRegion(srchsv[i,j,0],srchsv[i,j,1],srchsv[i,j,2]):
                tmp=clustering_model.predict(srchsv[i,j].reshape((-1,3)))[0]
                dsthsv[i,j]=clustering_model.cluster_centers_[tmp]
            else:
                dsthsv[i,j]=(0,0,0)
    dsthsv[:,:,0],dsthsv[:,:,1],dsthsv[:,:,2]=hsv_to_normalizedhsv(dsthsv[:,:,0],dsthsv[:,:,1],dsthsv[:,:,2])
    
    dstrgb=np.apply_along_axis(lambda a:colorsys.hsv_to_rgb(*a),2,dsthsv)*255
    return dstrgb


def imageanalyze(src,dst,colornum,colortarget):
    srcrgb=np.array(Image.open(src))
    hsv,model=getHSVImageAndClusteringModel(srcrgb,colornum,colortarget)
    dstrgb=replaceTargetColorRegion(hsv,model,colortarget)
    dstrgb=Image.fromarray(dstrgb.astype(np.uint8))
    dstrgb.save(dst)

if __name__=="__main__":
    colornum=4
    src="src.jpg"
    dst="dst.jpg"
    colortarget=HSVColorRegion()
    colortarget.setHueRegion(340,31)
    colortarget.setSaturationRegion(2,31)
    colortarget.setValueRegion(84,101)
    imageanalyze(src,dst,colornum,colortarget)