import axios from "axios"
import { useEffect } from "react"

export const DocumentLoadLink = ({res}) => {

    useEffect(() => {
        console.log(axios.defaults.baseURL)
    }, [])

    return <a href={`${axios.defaults.baseURL}/documents/${res}`} >Скачать</a>
} 